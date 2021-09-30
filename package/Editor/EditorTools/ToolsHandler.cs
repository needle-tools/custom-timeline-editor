using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Needle.Timeline
{
	public static class ToolsHandler
	{
		public static void RebuildInstances()
		{
			_requireRebuild = true;
			CreateToolInstances();
		}

		public static void Select(ICustomClipTool tool)
		{
			if (!_selected.Contains(tool))
				_selected.Add(tool);
		}

		public static void Deselect(ICustomClipTool tool)
		{
			_selected.RemoveAll(t => t == tool);
		}

		public static void DeselectAll()
		{
			_selected.Clear();
		}

		public static bool IsSelected(ICustomClipTool tool) => _selected.Contains(tool);

		private static readonly List<ICustomClipTool> _selected = new List<ICustomClipTool>();

		[InitializeOnLoadMethod]
		private static void Init()
		{
			EditorSceneManager.sceneOpened += OpenedScene;
			CreateToolInstances();
		}

		private static void OpenedScene(Scene scene, OpenSceneMode mode)
		{
			if (mode == OpenSceneMode.Single)
				RebuildInstances();
		}

		private static bool _requireRebuild = true;
		private static List<ICustomClipTool> _toolInstances;

		internal static IReadOnlyList<ICustomClipTool> ToolInstances
		{
			get
			{
				if (_requireRebuild) CreateToolInstances();
				return _toolInstances;
			}
		}

		private static void CreateToolInstances()
		{
			if (!_requireRebuild && _toolInstances != null) return;
			_requireRebuild = false;
			_toolInstances ??= new List<ICustomClipTool>();
			_toolInstances.Clear();
			var toolTypes = TypeCache.GetTypesDerivedFrom<ICustomClipTool>();
			foreach (var tool in toolTypes)
			{
				if (tool.IsAbstract || tool.IsInterface) continue;
				var instance = (ICustomClipTool)Activator.CreateInstance(tool);
				_toolInstances.Add(instance);
			}
		}
	}
}
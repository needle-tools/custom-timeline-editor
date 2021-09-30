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
		
		public static void RebuildInstances()
		{
			_requireRebuild = true;
			CreateToolInstances();
		}

		private static bool _requireRebuild = true;
		private static List<ICustomClipTool> _toolInstances;
		internal static IReadOnlyList<ICustomClipTool> ToolInstances
		{
			get
			{
				if(_requireRebuild) CreateToolInstances();
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
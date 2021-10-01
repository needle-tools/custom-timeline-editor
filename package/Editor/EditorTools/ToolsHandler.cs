using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
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
			_selected.RemoveAll(t =>
			{
				if (t != tool) return false;
				t.RemoveAllTargets();
				return true;
			});
		}

		public static void DeselectAll()
		{
			foreach(var sel in _selected) sel.RemoveAllTargets();
			_selected.Clear();
		}

		public static bool IsSelected(ICustomClipTool tool) => _selected.Contains(tool);

		private static readonly List<ICustomClipTool> _selected = new List<ICustomClipTool>();

		[InitializeOnLoadMethod]
		private static void Init()
		{
			EditorSceneManager.sceneOpened += OpenedScene;
			TimelineHooks.TimeChanged += OnTimeChanged;
			CreateToolInstances();
		}

		private static void OnTimeChanged(PlayableDirector obj)
		{
			foreach (var sel in _selected)
			{
				sel.RemoveAllTargets();
			}
			
			foreach (var tool in _selected)
			{
				foreach (var vm in ClipInfoViewModel.ActiveInstances)
				{
					foreach (var clip in vm.clips)
					{
						if (!clip.SupportedTypes.Any(tool.Supports)) continue;
						tool.AddTarget(vm, clip);
					}
				}
			}
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
﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	public static class ToolsHandler
	{
		public static void RebuildInstances()
		{
			_requireRebuild = true;
			CreateToolInstances();
		}

		private static readonly List<VisualElement> tempBuffer = new List<VisualElement>();

		public static void Select(ICustomClipTool tool)
		{
			if (_selected.Contains(tool)) return;
			_selected.Add(tool);

			var container = ToolsGUI.GetContainer(tool);
			#region Save added visual elements to be automatically removed on detach
			tempBuffer.Clear();
			tempBuffer.AddRange(container.Children());
			tool.Attach(container);

			var hasList = _attatched.ContainsKey(tool);
			var list = hasList ? _attatched[tool] : new List<VisualElement>();
			if (!hasList) _attatched.Add(tool, list);
			foreach (var ch in container.Children())
			{
				if (!tempBuffer.Contains(ch))
					list.Add(ch);
			}
			tempBuffer.Clear();
			#endregion

			foreach (var viewModel in ClipInfoViewModel.ActiveInstances)
			{
				foreach (var clip in viewModel.clips)
				{
					if (!clip.SupportedTypes.Any(tool.Supports)) continue;
					tool.AddTarget(viewModel, clip);
					if (tool is EditorTool et)
					{
						Debug.Log("Set active tool: " + et);
						ToolManager.SetActiveTool(et);
					}
				}
			}
		}

		public static void Deselect(ICustomClipTool tool)
		{
			_selected.RemoveAll(_ =>
			{
				if (_ != tool) return false;
				tool.RemoveAllTargets();
				var container = ToolsGUI.GetContainer(tool);
				tool.Detach(container);
				OnRemoveAttachedElements(tool);
				return true;
			});
		}

		public static void DeselectAll()
		{
			foreach (var tool in _selected)
			{
				tool.RemoveAllTargets();
				var container = ToolsGUI.GetContainer(tool);
				tool.Detach(container);
				OnRemoveAttachedElements(tool);
			}
			_selected.Clear();
		}

		private static void OnRemoveAttachedElements(ICustomClipTool tool)
		{
			if (_attatched.TryGetValue(tool, out var list))
			{
				foreach (var el in list)
				{
					if (el.parent != null)
						el.RemoveFromHierarchy();
				}
			}
		}

		public static bool IsSelected(ICustomClipTool tool) => _selected.Contains(tool);

		internal static IReadOnlyList<ICustomClipTool> ActiveTools => _selected;

		private static readonly List<ICustomClipTool> _selected = new List<ICustomClipTool>();

		private static readonly Dictionary<ICustomClipTool, IList<VisualElement>> _attatched
			= new Dictionary<ICustomClipTool, IList<VisualElement>>();

		[InitializeOnLoadMethod]
		private static void Init()
		{
			EditorSceneManager.sceneOpened += OpenedScene;
			TimelineHooks.TimeChanged += OnTimeChanged;
			ToolManager.activeToolChanged += OnActiveToolChanged;
			CreateToolInstances();
			UpdateToolTargets();

			TimelineWindowUtil.IsInit += () =>
			{
				var active = ToolManager.activeToolType;
				if (typeof(ICustomClipTool).IsAssignableFrom(active))
				{
					var inst = ToolInstances.FirstOrDefault(t => t.GetType() == active);
					inst.Select();
				}
			};
		}

		private static void OnActiveToolChanged()
		{
			if (!typeof(ICustomClipTool).IsAssignableFrom(ToolManager.activeToolType))
			{
				DeselectAll();
			}
		}

		private static void OnTimeChanged(PlayableDirector obj)
		{
			UpdateToolTargets();
		}

		private static void UpdateToolTargets()
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
				ICustomClipTool instance = default;
				if (typeof(EditorTool).IsAssignableFrom(tool))
				{
					instance = (ICustomClipTool)ScriptableObject.CreateInstance(tool);
					_toolInstances.Add(instance);
				}
				else
				{
					instance = (ICustomClipTool)Activator.CreateInstance(tool);
					_toolInstances.Add(instance);
				}
				ToolsSettings.HandleSettingsForToolInstance(instance);
			}
		}
	}
}
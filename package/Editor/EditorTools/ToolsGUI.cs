﻿using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	internal static class ToolsGUI
	{
		internal static VisualElement GetContainer(ICustomClipTool tool)
		{
			OnCreateContainerIfNecessary();
			return _toolsSettings;
			// if (_tools.TryGetValue(tool, out var container))
			// {
			// 	return container;
			// }
			// return null;
		}
		
		[InitializeOnLoadMethod] 
		private static void Init()
		{
			SceneView.beforeSceneGui += OnSceneGUI;
			ClipInfoViewModel.Created += vm => _recreateUI = true;
			OnCreateContainerIfNecessary();
		}

		private static readonly List<SceneView> _scenes = new List<SceneView>();
		private static VisualElement _root, _availableTools, _toolsSettings;
		private static bool _recreateUI = true;
		private static readonly Dictionary<ICustomClipTool, VisualElement> _tools = new Dictionary<ICustomClipTool, VisualElement>();

		private static void OnSceneGUI(SceneView obj)
		{
			OnCreateContainerIfNecessary();

			if (!_scenes.Contains(obj))
			{
				_scenes.Add(obj);
				obj.rootVisualElement.style.flexDirection = FlexDirection.ColumnReverse;
				obj.rootVisualElement.Add(_root);
				obj.rootVisualElement.Add(_toolsSettings);
			}
		}

		private static void OnCreateContainerIfNecessary()
		{
			if (_root == null)
			{
				_root = new VisualElement();
				_root.style.minWidth = 20;
				_root.style.maxWidth = new StyleLength(new Length(50, LengthUnit.Percent));
				_root.style.backgroundColor = EditorGUIUtility.isProSkin
					? new Color(0.21f, 0.21f, 0.21f, 0.8f)
					: new Color(0.8f, 0.8f, 0.8f, 0.8f);;
				_root.style.marginLeft = 5f;
				_root.style.marginBottom = 5f;
				_root.style.paddingTop = 5f;
				_root.style.paddingBottom = 5f;
				_root.style.paddingRight = 3f;
				_root.style.paddingLeft = 3f;
				_root.style.alignSelf = Align.FlexStart;
			}

			if (_availableTools == null)
			{
				_availableTools = new VisualElement();
				_availableTools.style.flexDirection = FlexDirection.Row;
				_root.Add(_availableTools);
			}

			if (_toolsSettings == null)
			{
				_toolsSettings = new VisualElement();
				_toolsSettings.style.minWidth = 0;
				_toolsSettings.style.maxWidth = new StyleLength(new Length(50, LengthUnit.Percent));
				_toolsSettings.style.backgroundColor = EditorGUIUtility.isProSkin
					? new Color(0.21f, 0.21f, 0.21f, 0.8f)
					: new Color(0.8f, 0.8f, 0.8f, 0.8f);;
				_toolsSettings.style.marginLeft = 5;
				_toolsSettings.style.marginBottom = 5;
				_toolsSettings.style.alignSelf = Align.FlexStart;
			}
			
			if (_recreateUI)
			{
				_recreateUI = false;
				_availableTools.Clear();
				foreach (var tool in ToolsHandler.ToolInstances)
				{
					if (!_tools.TryGetValue(tool, out var toolContainer))
					{
						toolContainer = new VisualElement();
						toolContainer.style.alignItems = Align.FlexStart;
						toolContainer.style.flexDirection = FlexDirection.Row;
						_tools.Add(tool, toolContainer);
					}
					else toolContainer.Clear();
				
					var name = tool.GetType().Name;
					var toolButton = new Button();
					toolButton.text = name;
					toolButton.style.flexGrow = 0;
					toolButton.style.height = 30;
					toolButton.AddManipulator(new ToolButtonManipulator(tool));
					toolContainer.Add(toolButton);
				
					_availableTools.Add(toolContainer);
				}
			}
		}
	}
}
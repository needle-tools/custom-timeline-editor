using System.Collections.Generic;
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
			if (_tools.TryGetValue(tool, out var container))
			{
				return container;
			}
			return null;
		}
		
		[InitializeOnLoadMethod] 
		private static void Init()
		{
			SceneView.beforeSceneGui += OnSceneGUI;
			ClipInfoViewModel.Created += vm => _recreateUI = true;
			OnCreateContainerIfNecessary();
		}

		private static readonly List<SceneView> _scenes = new List<SceneView>();
		private static VisualElement root, _toolsContainer;
		private static bool _recreateUI = true;
		private static readonly Dictionary<ICustomClipTool, VisualElement> _tools = new Dictionary<ICustomClipTool, VisualElement>();

		private static void OnSceneGUI(SceneView obj)
		{
			OnCreateContainerIfNecessary();

			if (!_scenes.Contains(obj))
			{
				_scenes.Add(obj);
				obj.rootVisualElement.Add(root);
				obj.rootVisualElement.style.flexDirection = FlexDirection.ColumnReverse;
			}
		}

		private static void OnCreateContainerIfNecessary()
		{
			if (root == null)
			{
				root = new VisualElement();
				root.style.minWidth = 20;
				root.style.maxWidth = new StyleLength(new Length(50, LengthUnit.Percent));
				var backgroundColor = EditorGUIUtility.isProSkin
					? new Color(0.21f, 0.21f, 0.21f, 0.8f)
					: new Color(0.8f, 0.8f, 0.8f, 0.8f);
				root.style.backgroundColor = backgroundColor;
				root.style.marginLeft = 10f;
				root.style.marginBottom = 10f;
				root.style.paddingTop = 5f;
				root.style.paddingRight = 5f;
				root.style.paddingLeft = 5f;
				root.style.paddingBottom = 5f;
			}

			if (_toolsContainer == null)
			{
				_toolsContainer = new VisualElement();
				root.Add(_toolsContainer);
			}
			
			if (_recreateUI)
			{
				_recreateUI = false;
				_toolsContainer.Clear();
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
					toolButton.AddManipulator(new ToolButtonManipulator(tool));
					toolContainer.Add(toolButton);
				
					_toolsContainer.Add(toolContainer);
				}
			}
		}
	}
}
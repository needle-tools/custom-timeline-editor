using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	internal static class ToolsGUI
	{
		[InitializeOnLoadMethod]
		private static void Init()
		{
			SceneView.beforeSceneGui += OnSceneGui;
			ClipInfoViewModel.Created += vm => _recreateUI = true;
		}

		private static readonly List<SceneView> _scenes = new List<SceneView>();
		private static VisualElement root, toolsContainer;
		private static bool _recreateUI = true;

		private static void OnSceneGui(SceneView obj)
		{
			OnBuildGUIIfNecessary();
			CreateButtonsIfNecessary();

			if (!_scenes.Contains(obj))
			{
				_scenes.Add(obj);
				obj.rootVisualElement.Add(root);
				obj.rootVisualElement.style.flexDirection = FlexDirection.ColumnReverse;
			}
		}

		private static void OnBuildGUIIfNecessary()
		{
			if (root != null) return;
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
			toolsContainer = new VisualElement();
			root.Add(toolsContainer);
		}

		private static void CreateButtonsIfNecessary()
		{
			if (!_recreateUI) return;
			_recreateUI = false;
			toolsContainer.Clear();
			foreach (var tool in ToolsHandler.ToolInstances)
			{
				var name = tool.GetType().Name;
				var toolButton = new Button();
				toolButton.style.height = 24f;
				toolButton.text = name;
				toolButton.style.flexGrow = 0;
				toolButton.AddManipulator(new ToolButtonManipulator(tool));
				toolsContainer.Add(toolButton);
			}
		}
	}
}
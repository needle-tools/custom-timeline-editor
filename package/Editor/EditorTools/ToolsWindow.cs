using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	public class ToolsWindow : EditorWindow
	{
		[MenuItem("Tools/Timeline Tools")]
		private static void Open()
		{
			if (HasOpenInstances<ToolsWindow>())
				FocusWindowIfItsOpen<ToolsWindow>();
			else
			{
				var window = CreateWindow<ToolsWindow>();
				window.Show();
			}
		}

		private void OnEnable()
		{
			titleContent = new GUIContent("Tools");
		}

		private void CreateGUI()
		{
			var root = rootVisualElement;
			var controls = new VisualElement();
			root.Add(controls);

			ControlsFactory.BuildControl("Test", true, controls);
			ControlsFactory.BuildControl("My Other Control", false, controls);


			// root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath("006df79959ca42f5b836147e5d456c46")));
			// var quickToolVisualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath("4023379787424b2ebf184f0e90ebc800"));
			// quickToolVisualTree.CloneTree(root);
		}
	}
}
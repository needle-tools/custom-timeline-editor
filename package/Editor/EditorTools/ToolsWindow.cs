using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	public class ToolsWindow : EditorWindow
	{
		public static VisualElement Root => global ??= new VisualElement();
		
		[MenuItem("Needle/Timeline/Tools")]
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

		private static VisualElement global;

		private void OnEnable()
		{
			titleContent = new GUIContent("Timeline Tools", AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath("1e4691ee0c9395e4492217851b315f09")));
		}

		private void CreateGUI()
		{
			var root = rootVisualElement;
			global ??= new VisualElement();
			root.Add(global);
		}
	}
}
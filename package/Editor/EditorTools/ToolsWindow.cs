using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	public class ToolsWindow : EditorWindow
	{
		public static VisualElement Root => global ??= new VisualElement();
		
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

		private static VisualElement global;

		private void OnEnable()
		{
			titleContent = new GUIContent("Tools");
		}

		private void CreateGUI()
		{
			var root = rootVisualElement;
			global ??= new VisualElement();
			root.Add(global);
		}
	}
}
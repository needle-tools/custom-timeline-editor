using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	internal class TestWindow : EditorWindow
	{
		[MenuItem("Tools/Timeline Tool TestWindow")]
		private static void Open()
		{
			if (HasOpenInstances<TestWindow>())
				FocusWindowIfItsOpen<TestWindow>();
			else
			{
				var window = CreateWindow<TestWindow>();
				window.Show();
			}
		}
		
		private void OnEnable()
		{
			titleContent = new GUIContent("Tools Tests");
		}

		private void CreateGUI()
		{
			var root = rootVisualElement;
			
			var controls = new VisualElement();
			root.Add(controls);
			new MockBinding("val", 123).BuildControl(controls);
			new MockBinding("MinMaxInteger", 50, true, new RangeAttribute(0,100)).BuildControl(controls);
			new MockBinding("my color", Color.red).BuildControl(controls);
			new MockBinding("weight", 1f).BuildControl(controls);
			new MockBinding("Weight Slider", 1f, true, new RangeAttribute(0,1)).BuildControl(controls);
			new MockBinding("pos2", new Vector2(1, 2)).BuildControl(controls);
			new MockBinding("position", new Vector3(1, 2, 3)).BuildControl(controls);
			new MockBinding("Point", new Vector4(1, 2, 3, 4)).BuildControl(controls);
			new MockBinding("Text", "test 123").BuildControl(controls);
			new MockBinding("Options", MyEnum.Option1).BuildControl(controls);


			// root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath("006df79959ca42f5b836147e5d456c46")));
			// var quickToolVisualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath("4023379787424b2ebf184f0e90ebc800"));
			// quickToolVisualTree.CloneTree(root);
		}

		private enum MyEnum
		{
			Option1,
			Option2
		}
	}
}
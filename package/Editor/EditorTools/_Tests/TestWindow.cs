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

			if (ControlsFactory.TryBuildToolPanel(out var panel, true))
			{
				root.Add(panel);
			}
			
			var controls = new Foldout(){text = "Controls"};
			root.Add(controls);
			controls.value = SessionState.GetBool("ControlsFoldout", true);
			controls.RegisterValueChangedCallback(v => SessionState.SetBool("ControlsFoldout", v.newValue));
			controls.Add(new MockBinding("val", 123).BuildControl());
			controls.Add(new MockBinding("MinMaxInteger", 50, true, new RangeAttribute(0,100)).BuildControl());
			controls.Add(new MockBinding("my color", Color.red).BuildControl());
			controls.Add(new MockBinding("hdr no alpha", Color.red, true, new ColorUsageAttribute(false, true)).BuildControl());
			controls.Add(new MockBinding("weight", 1f).BuildControl());
			controls.Add(new MockBinding("Weight Slider", 1f, true, new RangeAttribute(0,1)).BuildControl());
			controls.Add(new MockBinding("Weight Slider ^5", .5f, true, new PowerSlider(0,1, 5)).BuildControl());
			controls.Add(new MockBinding("pos2", new Vector2(1, 2)).BuildControl());
			controls.Add(new MockBinding("position", new Vector3(1, 2, 3)).BuildControl());
			controls.Add(new MockBinding("Point", new Vector4(1, 2, 3, 4)).BuildControl());
			controls.Add(new MockBinding("Text", "test 123").BuildControl());
			// new MockBinding("Options", MyEnum.Option1).BuildControl(controls);
			//
			// var foldout = new Foldout() { text = "My Foldout" };
			// controls.Add(foldout);
			// new MockBinding("some option", 1f).BuildControl(foldout);
			// new MockBinding("some option", "test 123").BuildControl(foldout);

		}

		private enum MyEnum
		{
			Option1,
			Option2
		}
	}
}
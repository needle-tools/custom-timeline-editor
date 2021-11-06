using UnityEditor;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	internal static class ControlsFactory
	{
		[InitializeOnLoadMethod]
		private static void Init()
		{
			controlAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath("a9727f46214640d1be592eb4e81682ee"));
			controlStyles = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath("e29516eda36d4ad1b6f8822975c7f21c"));
		}

		private static VisualTreeAsset controlAsset;
		private static StyleSheet controlStyles;
		 
		
		public static VisualElement BuildControl(string label, bool enabled, VisualElement target = null)
		{
			var instance = controlAsset.CloneTree();
			instance.styleSheets.Add(controlStyles);

			var toggle = instance.Q<Toggle>(null, "enabled");
			toggle.value = enabled;

			var name = instance.Q<Label>(null, "name"); 
			name.text = label;
			
			var controlContainer = instance.Q<VisualElement>(null, "control");
			var button = new Button(){text = "Hello"};
			controlContainer.Add(button);
			var button2 = new Button(){text = "Hello2"};
			controlContainer.Add(button2);

			controlContainer.SetEnabled(toggle.value);
			toggle.RegisterValueChangedCallback(evt => controlContainer.SetEnabled(evt.newValue));
			
			if(target != null) target.Add(instance);
			
			return instance;
		}
	}
}
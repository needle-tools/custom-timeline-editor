using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Needle.Timeline
{
	[CustomEditor(typeof(JsonContainer))]
	public class JsonContainerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			var t = target as JsonContainer;
			if (!t) return;
			EditorGUILayout.Space(5);
			EditorGUILayout.LabelField("Pretty", EditorStyles.boldLabel);
			var jsonFormatted = JToken.Parse(t.Content).ToString(Formatting.Indented);
			EditorGUILayout.TextArea(jsonFormatted, GUILayout.ExpandHeight(true));
		}
	}
}
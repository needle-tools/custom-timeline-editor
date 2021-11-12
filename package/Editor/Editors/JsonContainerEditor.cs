using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Needle.Timeline
{
	[CustomEditor(typeof(JsonContainer))]
	public class JsonContainerEditor : Editor
	{
		private string formatted, lastContent;

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			var t = target as JsonContainer;
			if (!t) return;
			EditorGUILayout.Space(5);
			EditorGUILayout.LabelField("Pretty", EditorStyles.boldLabel);
			UpdateIfNecessary();
			EditorGUILayout.TextArea(formatted, GUILayout.ExpandHeight(true));
		}

		private void UpdateIfNecessary()
		{
			var t = target as JsonContainer;
			if (!t) return;
			if (t.Content == lastContent) return;
			formatted = !string.IsNullOrEmpty(t.Content) ? JToken.Parse(t.Content).ToString(Formatting.Indented) : "<empty>";
			lastContent = t.Content;
		}
	}
}
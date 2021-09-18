using UnityEditor;
using UnityEngine;

namespace Needle.Timeline
{
	[CustomEditor(typeof(KeyframeInspectorHelper))]
	public class KeyframeInspectorHelperEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			var helper = target as KeyframeInspectorHelper;
			if (helper == null) return;
			if (helper.keyframe != null)
			{
				var kf = helper.keyframe;
				EditorGUILayout.LabelField(kf.time.ToString("0.00"));
				var valStr = kf.value?.ToString() ?? "null"; 
				EditorGUILayout.LabelField(new GUIContent(valStr, valStr));
			}
		}
	}
}
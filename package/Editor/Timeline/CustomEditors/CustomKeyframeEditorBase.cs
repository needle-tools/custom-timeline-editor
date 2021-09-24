using UnityEditor;
using UnityEngine;

namespace Needle.Timeline
{
	public abstract class CustomKeyframeEditorBase
	{
		public string Name { get; internal set; }
		public object Target { get; internal set; }
		
		public virtual void OnInspectorGUI()
		{
			DrawDefaultInspector(Name, Target);
		}

		internal static void DrawDefaultInspector(string name, object obj)
		{
			if (obj is ICustomKeyframe kf)
			{
				EditorGUILayout.LabelField(name);
				kf.time = EditorGUILayout.FloatField("Time", kf.time);
				var valStr = kf.value?.ToString() ?? "null"; 
				EditorGUILayout.LabelField(new GUIContent(valStr, valStr));
				// TypeCache.GetTypesWithAttribute<>()
			}
		}
	}
}
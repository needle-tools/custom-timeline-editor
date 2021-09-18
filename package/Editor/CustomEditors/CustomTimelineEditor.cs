using UnityEditor;
using UnityEngine;

namespace Needle.Timeline
{
	public abstract class CustomTimelineEditor
	{
		public object Target { get; internal set; }
		
		public virtual void OnInspectorGUI()
		{
			DrawDefaultInspector(Target);
		}

		internal static void DrawDefaultInspector(object obj)
		{
			if (obj is ICustomKeyframe kf)
			{
				EditorGUILayout.LabelField(kf.time.ToString("0.00"));
				var valStr = kf.value?.ToString() ?? "null"; 
				EditorGUILayout.LabelField(new GUIContent(valStr, valStr));
				// TypeCache.GetTypesWithAttribute<>()
			}
		}
	}
}
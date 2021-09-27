using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Needle.Timeline
{
	public struct KeyframeSelection
	{
		public ICustomClip Clip;
		public ICustomKeyframe Keyframe;
		public KeyframeMeta Meta;

		public struct KeyframeMeta
		{
			
		}
	}
	
	public abstract class CustomKeyframeEditorBase
	{
		public IList<KeyframeSelection> Target { get; internal set; }
		
		public virtual void OnInspectorGUI()
		{
			// DrawDefaultInspector(Target);
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
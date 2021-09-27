using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Needle.Timeline
{
	public struct SelectedKeyframe
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
		public IList<SelectedKeyframe> Target { get; } = new List<SelectedKeyframe>();
		
		public virtual void OnInspectorGUI()
		{
			DrawDefaultInspector(Target);
		}

		private static void DrawDefaultInspector(IList<SelectedKeyframe> keyframes)
		{
			foreach (var sel in keyframes)
			{
				DrawDefaultInspector(sel);
			}
		}

		private static StringBuilder _stringBuilder = new StringBuilder();
		internal static void DrawDefaultInspector(SelectedKeyframe selected)
		{
			if (selected.Keyframe != null)
			{
				var kf = selected.Keyframe;
				// EditorGUILayout.LabelField(name);
				kf.time = EditorGUILayout.FloatField("Time", kf.time);
				if (kf.value == null)
				{
					EditorGUILayout.LabelField("NULL");
				}
				else
				{
					_stringBuilder.Clear();
					StringHelper.GetTypeStringWithGenerics(kf.value.GetType(), _stringBuilder);
					var valStr = _stringBuilder.ToString();
					EditorGUILayout.LabelField("Value", valStr);
				}
				// TypeCache.GetTypesWithAttribute<>()
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Needle.Timeline
{
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

		private static readonly StringBuilder _stringBuilder = new StringBuilder();
		internal static void DrawDefaultInspector(SelectedKeyframe selected)
		{
			if (selected.Keyframe != null)
			{
				EditorGUILayout.HelpBox("Tip: You can double click a keyframe to jump to the keyframe time.", MessageType.None);
				
				var kf = selected.Keyframe;
				// EditorGUILayout.LabelField(name);
				var newTime = EditorGUILayout.FloatField("Time", kf.time);
				if (Math.Abs(newTime - kf.time) > float.Epsilon) kf.time = newTime;
				if (kf.value == null)
				{
					EditorGUILayout.LabelField("NULL");
				}
				else if (kf.value is string str)
				{
					EditorGUILayout.LabelField("Value", str);
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
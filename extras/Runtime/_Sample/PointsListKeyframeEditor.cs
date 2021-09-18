using System.Collections.Generic;
using Needle.Timeline;
using UnityEditor;
using UnityEngine;

namespace _Sample._Sample
{
	[CustomKeyframeEditor(typeof(ICustomKeyframe<List<Vector3>>))]
	public class PointsListKeyframeEditor : CustomTimelineEditor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			GUILayout.Space(10);
			if (Target is ICustomKeyframe<List<Vector3>> keyframe)
			{
				EditorGUILayout.LabelField("Count", keyframe.value?.Count.ToString() ?? "Empty");
			}
		}
	}
}
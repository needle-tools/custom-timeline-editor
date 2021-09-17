using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace Needle.Timeline
{
	[CustomTimelineEditor(typeof(CodeControlTrack))]
	public class CodeControlTrackEditor : TrackEditor
	{
		public override bool IsBindingAssignableFrom(Object candidate, TrackAsset track)
		{
			var go = candidate as GameObject;
			if (!go) return false;
			var anim = go.TryGetComponent(out IAnimated _);
			return anim;
		}
	}
}
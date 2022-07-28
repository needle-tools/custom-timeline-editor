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

		public override Object GetBindingFrom(Object candidate, TrackAsset track)
		{
			if (candidate is GameObject go) return go.GetComponent<IAnimated>() as Object;
			return candidate;
		}

		public override void OnCreate(TrackAsset track, TrackAsset copiedFrom)
		{
			Debug.Log("TODO: clone whole track");
			base.OnCreate(track, copiedFrom);
		}

		public override TrackDrawOptions GetTrackOptions(TrackAsset track, Object binding)
		{
			return base.GetTrackOptions(track, binding);
		}

		public override void OnTrackChanged(TrackAsset track)
		{
			base.OnTrackChanged(track);
		}
			
	}
}
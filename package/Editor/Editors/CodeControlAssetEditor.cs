using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace Needle.Timeline
{
	[CustomTimelineEditor(typeof(CodeControlAsset))]
	public class CodeControlAssetEditor : ClipEditor
	{
		public override void OnCreate(TimelineClip clip, TrackAsset track, TimelineClip clonedFrom)
		{
			base.OnCreate(clip, track, clonedFrom);
			Debug.Log("TODO: clone clips");
		}
	}
}
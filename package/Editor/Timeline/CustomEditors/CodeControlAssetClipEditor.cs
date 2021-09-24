using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace Needle.Timeline
{
	[CustomTimelineEditor(typeof(CodeControlAsset))]
	public class CodeControlAssetClipEditor : ClipEditor
	{
		public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region)
		{
			// if (GUI.Button(new Rect(region.position.position, new Vector2(100, 16)), "Hello"))
			// {
			// 	Event.current.Use();
			// 	Debug.Log("CLICK");
			// }
			base.DrawBackground(clip, region);
		}
	}
}
using UnityEditor.Timeline;
using UnityEngine;

namespace Needle.Timeline
{
	[CustomTimelineEditor(typeof(CodeControlTrack))]
	public class CodeControlCustomCurvesEditor : CustomCurvesEditor
	{
		public override void OnDrawHeader(Rect rect)
		{
			GUI.Label(rect, "Custom header");
		}

		public override void OnDrawTrack(Rect rect)
		{
			GUI.Label(rect, "Custom track " + ActiveRange);
			if (ActiveRange != null)
			{
				var clipRange= ActiveRange;
				var clipRect = new Rect();
				clipRect.xMin = clipRange.x;
				clipRect.xMax = clipRange.y;
				clipRect.height = 5;
				clipRect.y = rect.y;

				GUI.DrawTexture(clipRect, Texture2D.redTexture, ScaleMode.StretchToFill, false);
			}
		}
	}
}
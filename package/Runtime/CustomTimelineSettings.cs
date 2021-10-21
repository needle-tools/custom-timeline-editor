using UnityEditor;
using UnityEditor.Timeline;

namespace Needle.Timeline
{
	public class CustomTimelineSettings
	{
		// TODO: add settings file
		
		private static CustomTimelineSettings _instance;
		public static CustomTimelineSettings Instance => _instance ??= new CustomTimelineSettings();

		public bool AllowBuffering
		{
			get => SessionState.GetBool("CustomTimeline.AllowBuffer", false);
			set
			{
				if (AllowBuffering == value) return;
				SessionState.SetBool("CustomTimeline.AllowBuffer", value);
#pragma warning disable CS4014
				TimelineBuffer.RequestBufferCurrentInspectedTimeline();
#pragma warning restore CS4014
			}
		}

		public float DefaultBufferLenght = 2;
		
		public bool RenderOnionSkin = true;
	}
}
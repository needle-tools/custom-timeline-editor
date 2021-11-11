using UnityEditor;

namespace Needle.Timeline
{
	public class CustomTimelineSettings
	{
		// TODO: add settings file

		private static CustomTimelineSettings _instance;
		public static CustomTimelineSettings Instance => _instance ??= new CustomTimelineSettings();

		public bool AllowBuffering
		{
			get { return GlobalState.GetBool("CustomTimeline.AllowBuffer", false); }
			set
			{
				if (AllowBuffering == value) return;
				GlobalState.SetBool("CustomTimeline.AllowBuffer", value);
#if UNITY_EDITOR
#pragma warning disable CS4014
				TimelineBuffer.RequestBufferCurrentInspectedTimeline();
#pragma warning restore CS4014
#endif
			}
		}

		public float DefaultBufferLenght = 2;

		public bool RenderOnionSkin = true;
	}
}
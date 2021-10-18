using UnityEditor.Timeline;

namespace Needle.Timeline
{
	public class CustomTimelineSettings
	{
		private static CustomTimelineSettings _instance;
		public static CustomTimelineSettings Instance => _instance ??= new CustomTimelineSettings(); 
		
		public bool AllowBuffering = false;
		public bool RenderOnionSkin = true;
	}
}
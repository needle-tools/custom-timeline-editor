using UnityEditor;
using UnityEditor.Timeline;

namespace Needle.Timeline
{
	public static class TimelineWindowWatcher
	{
		private static void Init()
		{
			
			EditorWindow.HasOpenInstances<TimelineEditorWindow>();
		}
		
		
	}
}
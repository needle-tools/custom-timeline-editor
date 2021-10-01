using System.Linq;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;

namespace Needle.Timeline
{
	public static class TimelineWindowUtil
	{
		[InitializeOnLoadMethod]
		private static void Init()
		{
			if (EditorWindow.HasOpenInstances<TimelineEditorWindow>())
			{
				var directors = Object.FindObjectsOfType<PlayableDirector>();
				foreach(var dir in directors) dir.Evaluate();
			}
		}
		
		internal static bool TryRepaint()
		{
			var window = GetOrFindWindow();
			if (!window) return false;
			window.Repaint();
			return true; 
		}

		private static TimelineEditorWindow timelineWindow;
		private static TimelineEditorWindow GetOrFindWindow()
		{
			if (timelineWindow) return timelineWindow;
			if (!EditorWindow.HasOpenInstances<TimelineEditorWindow>()) return null;
			if (!timelineWindow)
			{
				timelineWindow = Resources.FindObjectsOfTypeAll<TimelineEditorWindow>().FirstOrDefault(t => t);
			}
			return timelineWindow;
		}
	}
}
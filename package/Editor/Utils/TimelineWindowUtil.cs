using System.Linq;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.UI;

namespace Needle.Timeline
{
	public static class TimelineWindowUtil
	{
		private static void Init()
		{
			EditorWindow.HasOpenInstances<TimelineEditorWindow>();
		}

		private static EditorWindow timelineWindow;

		internal static bool TryRepaint()
		{
			if (!EditorWindow.HasOpenInstances<TimelineEditorWindow>()) return false;

			if (!timelineWindow)
			{
				timelineWindow = Resources.FindObjectsOfTypeAll<TimelineEditorWindow>().FirstOrDefault(t => t);
				if (!timelineWindow) return false;
			}
			timelineWindow.Repaint();
			return true;
		}
	}
}
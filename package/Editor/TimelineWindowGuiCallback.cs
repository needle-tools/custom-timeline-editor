using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;

namespace Needle.Timeline
{
	internal static class TimelineWindowGuiCallback
	{

		[InitializeOnLoadMethod]
		private static void Init()
		{
			TimelineEditorWindow.CustomHeaderGUI += OnCustomTimelineGUI;
		}

		private static void OnCustomTimelineGUI()
		{
			var settings = CustomTimelineSettings.Instance;
			
			if (GUILayout.Button("Buffer " + (TimelineBuffer.Enabled ? "on" : "off")))
			{
				TimelineBuffer.Enabled = !TimelineBuffer.Enabled;
			}
			
			if(GUILayout.Button("Onion " + (settings.RenderOnionSkin ? "on" : "off")))
			{
				settings.RenderOnionSkin = !settings.RenderOnionSkin;
			}
		}
	}
}
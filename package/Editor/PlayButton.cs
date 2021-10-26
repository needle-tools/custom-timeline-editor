using Needle.Toolbar;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Playables;

namespace Needle.Timeline
{
	internal static class CustomTimelinePlayButton
	{
		[InitializeOnLoadMethod]
		private static void Init()
		{
			ToolbarButton button = default;
			button = new ToolbarButton("Play", () =>
			{
				var dir = TimelineEditor.masterDirector ? TimelineEditor.masterDirector : Object.FindObjectOfType<PlayableDirector>();
				if (dir)
				{
					switch (dir.state)
					{
						case PlayState.Paused:
							EditorUtility.SetDirty(dir);
							dir.Play();
							button.Label.text = "Pause";
							break;
						default:
							EditorUtility.SetDirty(dir);
							dir.Pause();
							button.Label.text = "Play";
							break;
					}
				}
			});
			button.Add();
		}
	}
}
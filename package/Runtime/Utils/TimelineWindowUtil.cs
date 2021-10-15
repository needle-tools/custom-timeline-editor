#if UNITY_EDITOR
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace Needle.Timeline
{
	public static class TimelineWindowUtil
	{
		[InitializeOnLoadMethod]
		private static async void Init()
		{
			// we need to cache the time because timeline window does not set it to the playable director before focus
			TimelineHooks.TimeChanged += OnTimeChanged;

			// TimelineEditor.GetInspectedTimeFromMasterTime(RefreshReason.SceneNeedsUpdate);
			// IsInit?.Invoke(); 

			//https://issuetracker.unity3d.com/issues/assembly-cache-should-be-empty-appears-in-the-console-when-evaluating-a-timeline-during-onenable


			await Task.Delay(1);
			var window = GetOrFindWindow();
			if (window)
			{ 
				// var directors = Object.FindObjectsOfType<PlayableDirector>();
				var dir = TimelineEditor.inspectedDirector;
				if(dir)
				{
					var lastTime = GetTime(); 
					if (lastTime >= 0)
					{
						dir.time = lastTime;
						TimelineHooks.CheckStateChanged(dir);
						Debug.Log(dir.time);
					}
					dir.Evaluate();
				}
				IsInit?.Invoke();
			}

			EditorApplication.update += OnEditorUpdate;
		}

		private static void OnEditorUpdate()
		{
			if (TimelineEditor.inspectedDirector)
				TimelineHooks.CheckStateChanged(TimelineEditor.inspectedDirector);
		}

		private static void OnTimeChanged(PlayableDirector obj, double d)
		{
			SetTime(obj);
		}

		private static void SetTime(PlayableDirector dir)
		{
			SessionState.SetFloat("Timeline_Time", (float)dir.time);
		}

		private static float GetTime()
		{
			return SessionState.GetFloat("Timeline_Time", -1);
		}

		internal static event Action IsInit;

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
#endif
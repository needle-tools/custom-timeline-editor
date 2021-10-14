﻿#if UNITY_EDITOR
using System;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;

namespace Needle.Timeline
{
	internal static class TimelineBuffer
	{
		[InitializeOnLoadMethod]
		private static void Init()
		{
			TimelineHooks.StateChanged += OnStateChanged;
			TimelineHooks.TimeChanged += OnTimeChanged;
		}

		private static int timeChangedCount;
		private static async void OnTimeChanged(PlayableDirector obj, double d)
		{
			if (isBuffering) return;
			var evtTime = d;
			var id = bufferRequestId;
			await Task.Delay(50);
			if (isBuffering || id != bufferRequestId) return;
			var diff = Mathf.Abs((float)(obj.time - evtTime));
			if(diff > .05f)
			{
				await RequestBufferCurrentInspectedTimeline(30);
			}
		}


		private static async void OnStateChanged(PlayableDirector dir, PlayState oldState)
		{
			if (oldState == PlayState.Paused && dir.state == PlayState.Playing)
			{
				// await Buffer(dir, 10);
			}
		}


		private static int bufferRequestId;
		internal static async Task RequestBufferCurrentInspectedTimeline(float seconds = 10, double? fromTime = null)
		{
			var id = ++bufferRequestId;
			await Task.Delay(200);
			if (id != bufferRequestId) return;
			var dir = TimelineEditor.inspectedDirector;
			if (dir != null && dir.state != PlayState.Playing)
				await Buffer(dir, seconds, fromTime);
		}

		private static bool isBuffering = false;
		internal static async Task Buffer(PlayableDirector dir, float seconds = 10, double? fromTime = null)
		{
			if (isBuffering) return;
			isBuffering = true;
			Debug.Log("BUFFER");
			
			foreach (var clip in ClipInfoViewModel.Instances)
			{
				if (clip.Script is IAnimatedEvents evt)
				{
					evt.OnReset();
				}
			}
			var targetTime = fromTime ?? dir.time;
			var startTime = targetTime - seconds;
			var frames = Mathf.CeilToInt(seconds * 120f);
			var framesPerSecond = Mathf.RoundToInt(1000 / 120f);
			IAnimatedExtensions.deltaTimeOverride =  1/120f;
			// var waitEveryFrame = Mathf.RoundToInt(60f / framesPerSecond);
			var state = dir.state;
			if (state != PlayState.Playing) dir.Play();
			for (var i = 0; i < frames; i++)
			{
				var time = (double)Mathf.Lerp((float)startTime, (float)targetTime, i / (float)frames);
				if (time < 0) continue;
				if (i == frames) time = targetTime;
				dir.time = time;
				dir.Evaluate();
				// if (i % 200 == 0)
				// {
				// 	await Task.Delay(1);
				// }
			}
			dir.time = targetTime;
			dir.Evaluate();
			IAnimatedExtensions.deltaTimeOverride = null;

			switch (state)
			{
				case PlayState.Paused:
					dir.Pause();
					break;
			}
			TimelineEditor.GetWindow().Repaint();
			isBuffering = false; 
		}
	}
}
#endif
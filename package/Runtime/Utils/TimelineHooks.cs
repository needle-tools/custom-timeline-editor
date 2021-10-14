using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine.Playables;

namespace Needle.Timeline
{
	public static class TimelineHooks
	{
		public static event Action<PlayableDirector, double> TimeChanged;
		public static event Action<PlayableDirector, PlayState> StateChanged;

		internal static void CheckStateChanged([NotNull] PlayableDirector dir)
		{
			if (dir == null) throw new ArgumentNullException(nameof(dir));
			if (_lastStates.TryGetValue(dir, out var state))
			{ 
				if (Math.Abs(state.time - dir.time) > 0.005f)
				{
					var lastTime = state.time;
					state.time = dir.time;
					TimeChanged?.Invoke(dir, lastTime);
				}
				var lastState = state.state;
				state.state = dir.state;
				if (lastState != dir.state)
				{
					StateChanged?.Invoke(dir, lastState);
				}
			}
			else
			{
				_lastStates.Add(dir, new PlayableDirectorState()
				{
					time = dir.time,
					state = dir.state
				});
			}
		}

		private static readonly Dictionary<PlayableDirector, PlayableDirectorState> _lastStates = new Dictionary<PlayableDirector, PlayableDirectorState>();

		private class PlayableDirectorState
		{
			public double time;
			public PlayState state;
		}
	}
}
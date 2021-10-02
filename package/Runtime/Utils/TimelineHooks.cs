using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine.Playables;

namespace Needle.Timeline
{
	public static class TimelineHooks
	{
		public static event Action<PlayableDirector> TimeChanged;

		internal static void CheckTimeChanged([NotNull] PlayableDirector dir)
		{
			if (dir == null) throw new ArgumentNullException(nameof(dir));
			if (_lastStates.TryGetValue(dir, out var state))
			{
				_lastStates[dir] = dir.time;
				if (Math.Abs(state - dir.time) > 0.01f)
				{
					TimeChanged?.Invoke(dir);
				}
			}
			else _lastStates.Add(dir, dir.time);
		}

		private static readonly Dictionary<PlayableDirector, double> _lastStates = new Dictionary<PlayableDirector, double>();
	}
}
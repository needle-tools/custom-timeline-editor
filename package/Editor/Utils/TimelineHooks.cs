using System;
using UnityEngine.Playables;

namespace Needle.Timeline
{
	public static class TimelineHooks
	{
		public static event Action<PlayableDirector> Changed;

		internal static void NotifyChanged(ClipInfoViewModel viewModel)
		{
			Changed?.Invoke(viewModel.director);
		}
	}
}
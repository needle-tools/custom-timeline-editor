using UnityEngine.Playables;

namespace Needle.Timeline.Commands
{
	public class TimelineModifyTime : Command
	{
		private readonly PlayableDirector dir;
		private readonly double newTime;
		private readonly double oldTime;

		public TimelineModifyTime(PlayableDirector dir, double newTime)
		{
			this.dir = dir;
			this.oldTime = dir.time;
			this.newTime = newTime;
		}

		protected override void OnRedo()
		{
			dir.time = newTime;
#if UNITY_EDITOR
			TimelineWindowUtil.TryRepaint();
			dir.Evaluate();
#endif
		}

		protected override void OnUndo()
		{
			dir.time = oldTime;
#if UNITY_EDITOR
			TimelineWindowUtil.TryRepaint();
			dir.Evaluate();
#endif
		}
	}
}
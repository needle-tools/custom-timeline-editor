using UnityEngine.Playables;

namespace Needle.Timeline
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
			TimelineWindowUtil.TryRepaint();
		}

		protected override void OnUndo()
		{
			dir.time = oldTime;
			TimelineWindowUtil.TryRepaint();
		}
	}
}
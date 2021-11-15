namespace Needle.Timeline.Commands
{
	public class KeyframeModifyTime : Command
	{
		internal readonly float previousTime;
		internal readonly ICustomKeyframe keyframe;
		internal float? newTime;

		public override bool IsValid => newTime != null;

		public KeyframeModifyTime(ICustomKeyframe keyframe)
		{
			this.keyframe = keyframe;
			this.previousTime = keyframe.time;
		}

		protected override void OnRedo()
		{
			if (newTime != null)
				keyframe.time = newTime.Value;
		}

		protected override void OnUndo()
		{
			keyframe.time = previousTime;
		}
	}
}
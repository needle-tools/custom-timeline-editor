namespace Needle.Timeline
{
	public class ModifyTime : Command
	{
		private readonly ICustomKeyframe keyframe;
		private readonly float previousTime;
		private readonly float newTime;

		public ModifyTime(ICustomKeyframe keyframe, float newTime)
		{
			this.keyframe = keyframe;
			this.previousTime = keyframe.time;
			this.newTime = newTime;
		}

		protected override void OnExecute()
		{
			keyframe.time = newTime;
		}

		protected override void OnUndo()
		{
			keyframe.time = previousTime;
		}
	}
}
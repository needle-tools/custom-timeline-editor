using System.Collections.Generic;

namespace Needle.Timeline
{
	public class ModifyTime : Command
	{
		internal readonly float previousTime;
		internal readonly ICustomKeyframe keyframe;
		internal float? newTime;

		public override bool IsValid => newTime != null;

		public ModifyTime(ICustomKeyframe keyframe)
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
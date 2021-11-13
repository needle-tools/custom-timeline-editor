namespace Needle.Timeline
{
	public class KeyframeModifyEasing : Command
	{
		private readonly ICustomKeyframe keyframe;
		private readonly float easeIn;
		private readonly float easeOut;
		private float newEaseIn;
		private float newEaseOut;

		public override bool IsValid => keyframe != null && base.IsValid;

		public KeyframeModifyEasing(ICustomKeyframe keyframe)
		{
			this.keyframe = keyframe;
			easeIn = keyframe.easeInWeight;
			easeOut = keyframe.easeOutWeight;
		}

		protected override void OnCaptureState()
		{
			base.OnCaptureState();
			newEaseIn = keyframe.easeInWeight;
			newEaseOut = keyframe.easeOutWeight;
			this.IsDone = true;
		}

		protected override void OnRedo()
		{
			keyframe.easeInWeight = newEaseIn;
			keyframe.easeOutWeight = newEaseOut;
		}

		protected override void OnUndo()
		{
			keyframe.easeInWeight = easeIn;
			keyframe.easeOutWeight = easeOut;
		}
	}
}
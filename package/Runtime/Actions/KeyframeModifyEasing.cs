namespace Needle.Timeline
{
	public class KeyframeModifyEasing : Command, ICapture
	{
		private readonly ICustomKeyframe keyframe;
		private readonly float easeIn;
		private readonly float easeOut;
		private float newEaseIn;
		private float newEaseOut;
		private bool captured;

		public override bool IsValid => captured && keyframe != null && base.IsValid;

		public KeyframeModifyEasing(ICustomKeyframe keyframe)
		{
			this.keyframe = keyframe;
			easeIn = keyframe.easeInWeight;
			easeOut = keyframe.easeOutWeight;
		}

		public void CaptureState()
		{
			if (captured) return;
			captured = true;
			newEaseIn = keyframe.easeInWeight;
			newEaseOut = keyframe.easeOutWeight;
			this.IsDone = true;
		}

		protected override void OnRedo()
		{
			if (!captured) return;
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
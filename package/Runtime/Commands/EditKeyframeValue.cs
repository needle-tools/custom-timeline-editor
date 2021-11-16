using System;
using JetBrains.Annotations;

namespace Needle.Timeline.Commands
{
	public class EditKeyframeValue : Command
	{
		private readonly ICustomKeyframe keyframe;
		private object lastValue;
		private object nextValue;

		public bool IsKeyframe(ICustomKeyframe kf) => kf == keyframe;
		
		public EditKeyframeValue([NotNull] ICustomKeyframe keyframe)
		{
			this.keyframe = keyframe ?? throw new ArgumentNullException(nameof(keyframe));
			lastValue = CloneUtil.TryClone(this.keyframe.value);
			IsDone = true;
		}

		protected override void OnRedo()
		{
			lastValue = CloneUtil.TryClone(keyframe.value);
			keyframe.value = nextValue;
			// keyframe.RaiseValueChangedEvent();
		}

		protected override void OnUndo()
		{
			if (nextValue == null) nextValue = CloneUtil.TryClone(keyframe.value);
			keyframe.value = lastValue;
			// keyframe.RaiseValueChangedEvent();
		}
	}
}
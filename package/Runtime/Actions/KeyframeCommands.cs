﻿namespace Needle.Timeline
{
	public class CreateKeyframe : Command
	{
		private readonly ICustomKeyframe keyframe;
		private readonly ICustomClip clip;

		public CreateKeyframe(ICustomKeyframe keyframe, ICustomClip clip, bool done = false) : base(done)
		{
			this.keyframe = keyframe;
			this.clip = clip;
		}
		
		protected override void OnRedo()
		{
			clip.Add(keyframe);
		}

		protected override void OnUndo()
		{
			clip.Remove(keyframe);
		}
	}
	
	public class DeleteKeyframe : Command
	{
		private readonly ICustomKeyframe keyframe;
		private readonly ICustomClip clip;

		public DeleteKeyframe(ICustomKeyframe keyframe, ICustomClip clip, bool done = false) : base(done)
		{
			this.keyframe = keyframe;
			this.clip = clip;
		}
		
		protected override void OnRedo()
		{
			clip.Remove(keyframe);
		}

		protected override void OnUndo()
		{
			clip.Add(keyframe);
		}
	}
}
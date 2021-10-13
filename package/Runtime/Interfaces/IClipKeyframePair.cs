using UnityEngine;

namespace Needle.Timeline
{
	public interface IClipKeyframePair
	{
		ICustomClip Clip { get; set; }
		ICustomKeyframe Keyframe { get; set; }
	}

	public struct ClipKeyframePair : IClipKeyframePair
	{
		public ICustomClip Clip { get; set; }
		public ICustomKeyframe Keyframe { get; set; }

		public ClipKeyframePair(ICustomClip clip, ICustomKeyframe keyframe)
		{
			this.Clip = clip;
			this.Keyframe = keyframe;
		}
	}
}
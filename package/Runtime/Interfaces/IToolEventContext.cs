namespace Needle.Timeline
{
	public interface IToolEventContext
	{
		
	}

	public class KeyframeEventContext : IToolEventContext
	{
		public readonly float Time;
		public readonly object Value;

		public KeyframeEventContext(float time, object value)
		{
			Time = time;
			Value = value;
		}

		public static KeyframeEventContext Create(IReadonlyCustomKeyframe keyframe)
		{
			return new KeyframeEventContext(keyframe.time, keyframe.value);
		}
	}
}
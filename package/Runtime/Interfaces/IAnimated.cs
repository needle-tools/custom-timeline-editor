namespace Needle.Timeline
{
	public interface IAnimated
	{
		
	}

	public readonly struct FrameInfo
	{
		public readonly float Time;
		public readonly float DeltaTime;

		public FrameInfo(float time, float deltaTime)
		{
			Time = time;
			DeltaTime = deltaTime;
		}

		public static FrameInfo Now() => new FrameInfo(UnityEngine.Time.time, UnityEngine.Time.deltaTime);
	}

	public interface IAnimatedEvents
	{
		void OnReset();
		void OnEvaluated(FrameInfo frame);
	}
}
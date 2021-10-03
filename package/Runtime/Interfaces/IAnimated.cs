namespace Needle.Timeline
{
	public interface IAnimated
	{
		
	}

	public class FrameInfo
	{
		public float Time { get; internal set; }
		public float DeltaTime { get; internal set; }

		public FrameInfo(float time, float deltaTime)
		{
			Time = time;
			DeltaTime = deltaTime;
		}
	}

	public interface ITimelineUpdateCallback
	{
		void OnEvaluated(FrameInfo frame);
	}
}
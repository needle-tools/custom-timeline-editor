namespace Needle.Timeline.CurveEasing
{
	[Priority(100)]
	public class DefaultEasing : ICurveEasing
	{
		public float Modify(float value)
		{
			return value;
		}
	}
}
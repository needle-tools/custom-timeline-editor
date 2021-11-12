namespace Needle.Timeline.CurveEasing
{
	[Priority(100)]
	public class NoEasing : ICurveEasing
	{
		public float Modify(float value)
		{
			return value;
		}
	}
}
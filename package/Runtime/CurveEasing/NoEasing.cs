namespace Needle.Timeline.CurveEasing
{
	public class NoEasing : ICurveEasing
	{
		public float Modify(float value)
		{
			return value;
		}
	}
}
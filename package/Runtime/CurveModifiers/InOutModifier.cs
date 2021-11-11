namespace Needle.Timeline.CurveModifiers
{
	public class QuadraticInOutModifier : ICurveEasing
	{
		public float Modify(float value)
		{
			if ((value *= 2f) < 1f) return 0.5f*value*value;
			return -0.5f*((value -= 1f)*(value - 2f) - 1f);
		}
	}
}
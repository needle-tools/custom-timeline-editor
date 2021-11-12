namespace Needle.Timeline.CurveEasing
{
	// https://gist.github.com/Fonserbc/3d31a25e87fdaa541ddf
	public class BounceEasing : ICurveEasing
	{
		public float Modify(float value)
		{
			if (value < 0.5f) return In(value*2f)*0.5f;
			return Out(value*2f - 1f)*0.5f + 0.5f;
		}
		
		public static float In (float k) {
			return 1f - Out(1f - k);
		}
		
		public static float Out (float k)
		{
			if (k < (1f/2.75f)) {
				return 7.5625f*k*k;				
			}
			if (k < (2f/2.75f)) {
				return 7.5625f*(k -= (1.5f/2.75f))*k + 0.75f;
			}
			if (k < (2.5f/2.75f)) {
				return 7.5625f *(k -= (2.25f/2.75f))*k + 0.9375f;
			}
			return 7.5625f*(k -= (2.625f/2.75f))*k + 0.984375f;
		}
	}
}
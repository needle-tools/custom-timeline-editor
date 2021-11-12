using UnityEngine;

namespace Needle.Timeline.CurveEasing
{
	public class CircularInOutEasing : ICurveEasing
	{
		// https://github.com/acron0/Easings/blob/master/Easings.css
		public float Modify(float value)
		{
			if(value < 0.5f)
			{
				return 0.5f * (1 - Mathf.Sqrt(1 - 4 * (value * value)));
			}
			return 0.5f * (Mathf.Sqrt(-((2 * value) - 3) * ((2 * value) - 1)) + 1);
		}
	}
}
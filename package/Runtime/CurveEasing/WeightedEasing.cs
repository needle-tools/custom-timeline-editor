using UnityEngine;

namespace Needle.Timeline.CurveEasing
{
	public class WeightedEasing : ICurveEasing, IWeighted
	{
		
		public float Weight { get; set; }

		public float Modify(float value)
		{
			if (Weight > .5f)
			{
				var t = Weight * 2f - 1;
				return Mathf.Lerp(value, 1, value * t);
			}
			else
			{
				var t = Weight * 2;
				return Mathf.Lerp(Mathf.Pow(value, 3), value, t); 
			}
		}

	}
}
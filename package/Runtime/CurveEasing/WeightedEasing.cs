using Newtonsoft.Json;
using UnityEngine;

namespace Needle.Timeline.CurveEasing
{
	public class WeightedEasing : ICurveEasing, IWeighted
	{
		public float Weight { get; set; } = .5f;
		
		public float Modify(float value)
		{
			return ApplyWeight(value, Weight);
		}

		public static float ApplyWeight(float value, float weight)
		{
			if (weight > .5f)
			{
				var t = weight * 2f - 1;
				return Mathf.Lerp(value, 1, value * t);
			}
			else
			{
				var t = weight * 2;
				return Mathf.Lerp(Mathf.Pow(value, 3), value, t); 
			}
		}

	}
}
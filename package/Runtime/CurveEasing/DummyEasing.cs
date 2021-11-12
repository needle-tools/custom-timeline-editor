using UnityEngine;

namespace Needle.Timeline.Easings
{
	public class FixedEasing : ICurveEasing
	{
		public float Position = .5f;
		
		public float Modify(float value)
		{
			return Mathf.Clamp01(Position); 
		}
	}
}
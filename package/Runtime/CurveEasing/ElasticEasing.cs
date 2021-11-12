using System;
using UnityEngine;

namespace Needle.Timeline.CurveEasing
{
	// https://github.com/acron0/Easings/blob/master/Easings.cs
	public class ElasticEasing : ICurveEasing
	{
		private const float HALFPI = Mathf.PI / 2f;
		
		public float Modify(float value)
		{
			if(value < 0.5f)
			{
				return 0.5f * Mathf.Sin(13 * HALFPI* (2 * value)) * Mathf.Pow(2, 10 * ((2 * value) - 1));
			}
			return 0.5f * (Mathf.Sin(-13 * HALFPI * ((2 * value - 1) + 1)) * Mathf.Pow(2, -10 * (2 * value - 1)) + 2);
		}
	}
}
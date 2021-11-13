using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Needle.Timeline.CurveEasing
{
	public class RandomEasing : ICurveEasing
	{
		private float currentRandom = .5f;
		private float lastRandomTime;
		private float targetRandom = .5f;
		
		public float Modify(float value)
		{
			var t = 1 - Mathf.Abs(2 * (value - .5f));
			if (Time.time - lastRandomTime > .5f)
			{
				lastRandomTime = Time.time;
				targetRandom = Random.value;
			}
			currentRandom = Mathf.Lerp(currentRandom, targetRandom, Time.deltaTime*2);
			return Mathf.Lerp(value, currentRandom, t);
		}
	}
}
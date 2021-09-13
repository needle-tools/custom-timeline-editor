using System;
using UnityEngine;

namespace Needle.Timeline
{
	public struct AnimationCurveWrapper : ICustomClip<float>
	{
		private readonly Func<AnimationCurve> curve;
		private readonly string m_Name;

		public AnimationCurveWrapper(Func<AnimationCurve> curve, string name)
		{
			this.curve = curve;
			m_Name = name;
		}

		public float Evaluate(float time)
		{
			var res = curve().Evaluate(time);
			// Debug.Log(m_Name + " has " + res + " at " + time);
			return res;
		}

		object ICustomClip.Evaluate(float time)
		{
			return Evaluate(time);
		}
	}
}
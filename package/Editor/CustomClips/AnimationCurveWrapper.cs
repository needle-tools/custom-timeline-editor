using System;
using System.Collections.Generic;
using UnityEngine;

namespace Needle.Timeline
{
	public struct AnimationCurveWrapper : ICustomClip<float>
	{
		private readonly Func<AnimationCurve> curve;
		
		public string Name { get; set; }
		
		public AnimationCurveWrapper(Func<AnimationCurve> curve, string name)
		{
			this.curve = curve;
			Name = name;
			Changed = default;
			Keyframes = null;
			SupportedTypes = new[] { typeof(float) };
		}


		public float Evaluate(float time)
		{
			var res = curve().Evaluate(time);
			// Debug.Log(m_Name + " has " + res + " at " + time);
			return res;
		}

		public bool CanAdd(Type type)
		{
			return typeof(float).IsAssignableFrom(type);
		}

		public bool CanAdd(ICustomKeyframe kf)
		{
			return kf.value?.GetType() == typeof(float);
		}

		public bool Add(ICustomKeyframe kf)
		{
			return false;
		}

		public void Remove(ICustomKeyframe kf)
		{
			
		}

		public event Action Changed;
		public IReadOnlyCollection<IReadonlyCustomKeyframe> Keyframes { get; }
		public ICustomKeyframe GetPrevious(float time)
		{
			return null;
		}

		public ICustomKeyframe GetClosest(float time)
		{
			return null;
		}

		public Type[] SupportedTypes { get; private set; }

		object ICustomClip.Evaluate(float time)
		{
			return Evaluate(time);
		}

		public bool CanInterpolate(Type type)
		{
			return true;
		}

		public object Interpolate(object v0, object v1, float t)
		{
			var float0 = (float)v0;
			var float1 = (float)v1;
			return Mathf.Lerp(float0, float1, t);
		}
	}
}
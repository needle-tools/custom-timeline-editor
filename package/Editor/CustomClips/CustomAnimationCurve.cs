using System.Collections.Generic;
using Editor.Interfaces;

namespace Needle.Timeline
{
	public class CustomAnimationCurve<T> : ICustomClip<T>
	{
		private readonly ICanInterpolate<T> _interpolator;
		private readonly List<ICustomKeyframe<T>> _keyframes;

		public CustomAnimationCurve(ICanInterpolate<T> interpolator, List<ICustomKeyframe<T>> keyframes = null)
		{
			this._interpolator = interpolator;
			this._keyframes = keyframes ?? new List<ICustomKeyframe<T>>();
		}

		public T Evaluate(float time)
		{
			for (var index = 0; index < _keyframes.Count; index++)
			{
				var current = _keyframes[index];
				if (current.time <= time)
				{
					// if this is the last keyframe return its value
					if (index + 1 >= _keyframes.Count) return current.value;
					var next = _keyframes[index + 1];
					// if the next keyframe is also <= time we have not found the closest keyframe yet
					if (next.time < time) continue;
					// interpolate between this and the next keyframe
					var t = GetPosition(time, current.time, next.time);
					return _interpolator.Interpolate(current.value, next.value, t);
				}
				
				// if no keyframe was found that is <= time
				if (index + 1 >= _keyframes.Count)
				{
					return current.value;
				}
			}

			return default;
		}

		object ICustomClip.Evaluate(float time)
		{
			return Evaluate(time);
		}
		

		private static float GetPosition(float current, float start, float end)
		{
			var diff = end - start;
			current -= start;
			current /= diff;
			return current;
		}
	}
}
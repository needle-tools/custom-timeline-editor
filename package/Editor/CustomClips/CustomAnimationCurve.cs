using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.Profiling;
using UnityEngine;

namespace Needle.Timeline
{
	public class CustomAnimationCurve<T> : ICustomClip<T>, IInterpolator<T>, IKeyframesProvider, IHasInterpolator
	{
		private IInterpolator<T> _interpolator;
		private readonly List<ICustomKeyframe<T>> _keyframes;
		private readonly ProfilerMarker _evaluateMarker = new ProfilerMarker("CustomAnimationCurve Evaluate " + typeof(T));
		private readonly ProfilerMarker _interpolationMarker = new ProfilerMarker("CustomAnimationCurve Interpolate " + typeof(T));

		public string Name { get; set; }
		
		[JsonIgnore]
		public IInterpolator Interpolator
		{
			get => (IInterpolator)_interpolator;
			set => _interpolator = (IInterpolator<T>)value;
		}
		
		public IEnumerable<ICustomKeyframe> Keyframes => _keyframes;
		
		public ICustomKeyframe GetPrevious(float time)
		{
			return FindKeyframe(time, true);
		}

		public ICustomKeyframe GetClosest(float time)
		{
			return FindKeyframe(time, false);
		}

		public T Interpolate(T v0, T v1, float t)
		{
			return _interpolator.Interpolate(v0, v1, t);
		}
		
		public bool CanInterpolate(Type type)
		{
			return type == typeof(T);
		}

		object IInterpolator.Interpolate(object v0, object v1, float t)
		{
			return Interpolate((T)v0, (T)v1, t);
		}

		public CustomAnimationCurve()
		{
			this.Name = "unnamed";
			this._interpolator = null;
			this._keyframes = new List<ICustomKeyframe<T>>();
		}

		public T Evaluate(float time)
		{
			if (!didRegisterKeyframeEvents) RegisterKeyframeEvents();
			
			if (keyframesTimeChanged || keyframeAdded)
			{
				keyframeAdded = false;
				keyframesTimeChanged = false;
				_keyframes.Sort((k1, k2) => Mathf.RoundToInt((k1.time - k2.time) * 100_00));
			}
			
			using (_evaluateMarker.Auto())
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
						using(_interpolationMarker.Auto())
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
		}

		public bool Add(ICustomKeyframe kf)
		{
			if (kf == null || kf.value == null || !(kf is ICustomKeyframe<T> keyframe))
				return false;
			_keyframes.Add(keyframe);
			keyframeAdded = true;
			RegisterKeyframeEvents(keyframe);
			Changed?.Invoke();
			return true;
		}

		public void Remove(ICustomKeyframe kf)
		{
			if (kf == null || kf.value == null || !(kf is ICustomKeyframe<T> keyframe))
				return;
			if (_keyframes.Remove(keyframe))
			{
				UnregisterKeyframeEvents(keyframe);
				Changed?.Invoke();
			}
		}

		public event Action Changed;
		IReadOnlyCollection<IReadonlyCustomKeyframe> ICustomClip.Keyframes => _keyframes;

		object ICustomClip.Evaluate(float time)
		{
			return Evaluate(time);
		}

		private bool keyframesTimeChanged, keyframeAdded;
		private bool didRegisterKeyframeEvents;

		private void RegisterKeyframeEvents()
		{
			didRegisterKeyframeEvents = true;
			foreach (var kf in _keyframes)
				RegisterKeyframeEvents(kf);
		}

		private void RegisterKeyframeEvents(ICustomKeyframe kf)
		{
			kf.TimeChanged += OnKeyframeTimeChanged;
			kf.ValueChanged += OnKeyframeValueChanged;
		}

		private void UnregisterKeyframeEvents(ICustomKeyframe kf)
		{
			kf.TimeChanged -= OnKeyframeTimeChanged;
			kf.ValueChanged -= OnKeyframeValueChanged;
		}

		private void OnKeyframeValueChanged()
		{
			Debug.Log("Keyframe changed");
			Changed?.Invoke();
		}

		private void OnKeyframeTimeChanged()
		{
			keyframesTimeChanged = true;
			Changed?.Invoke();
		}


		private ICustomKeyframe FindKeyframe(float time, bool onlyPrevious)
		{
			ICustomKeyframe closest = default;
			var closestDelta = double.MaxValue;
			foreach (var kf in _keyframes)
			{
				if (onlyPrevious && kf.time > time) continue;
				var delta = Mathf.Abs(time - kf.time);
				if (delta < closestDelta)
				{
					closest = kf;
					closestDelta = delta;
				}
			}
			return closest;
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
﻿using System;
using System.Collections.Generic;
using DefaultNamespace;
using JetBrains.Annotations;
using UnityEditor.Build;
using UnityEngine;

namespace Needle.Timeline
{
	public interface ICustomKeyframe
	{
		object value { get; set; }
		float time { get; set; }
	}

	public interface ICustomKeyframe<T> : ICustomKeyframe
	{
		new T value { get; set; }
	}

	public interface ICustomClip
	{
		object Evaluate(float time);
		// T Evaluate<T>(float time);
	}

	public interface ICustomClip<out T> : ICustomClip
	{
		new T Evaluate(float time);
	}

	[Serializable]
	public class PointsKeyframe : ICustomKeyframe<List<Vector3>>
	{
		object ICustomKeyframe.value
		{
			get => value;
			set
			{
				if (!(value is List<Vector3> _value))
					throw new InvalidOperationException();
				else
					this.value = _value;
			}
		}

		private List<Vector3> floats;

		public List<Vector3> value { get; set; }
		public float time { get; set; }
	}

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

	public class PointClip : ICustomClip<List<Vector3>>
	{
		public List<PointsKeyframe> keyframes;

		public List<Vector3> Evaluate(float time)
		{
			if (keyframes?.Count > 0)
				return keyframes[0].value;
			return null;
		}

		object ICustomClip.Evaluate(float time)
		{
			return Evaluate(time);
		}
	}
}
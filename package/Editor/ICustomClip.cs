﻿using System;
using System.Collections.Generic;
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
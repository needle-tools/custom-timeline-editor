﻿#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Needle.Timeline
{
	public class CustomKeyframe<T> : ICustomKeyframe<T>, ICloneable
	{
		private float time1;
		private T value1 = default!;
		private int lastHash;

		object? ICustomKeyframe.value
		{
			get => value;
			set
			{
				if(value?.GetType() == typeof(T))
					this.value = (T)value;
			}
		}

		public T value
		{
			get => value1;
			set
			{
				if (value == null && value1 == null || (value?.Equals(value1) ?? false)) return;
				value1 = value;
				ValueChanged?.Invoke();
			}
		}

		object? IReadonlyCustomKeyframe.value => value;

		public float time
		{
			get => time1;
			set
			{
				if (Math.Abs(value - time1) < 0.00000001f) return;
				time1 = value;
				TimeChanged?.Invoke();
			}
		}

		public event Action? TimeChanged;
		public event Action? ValueChanged;
		
		public void RaiseValueChangedEvent()
		{
			ValueChanged?.Invoke();
		}

		private readonly Type[] types = { typeof(T) };
		public Type[] AcceptedTypes()
		{
			return types;
		}

		// ReSharper disable once UnusedMember.Global
		public CustomKeyframe()
		{
			// required to be able to create via activator
		}

		public CustomKeyframe(T value, float time)
		{
			this.value = value;
			this.time = time;
		}

		public object Clone()
		{
			T clonedValue = default;
			try
			{
				clonedValue = (T)CloneUtil.TryClone(value);
			}
			catch (CouldNotCloneException)
			{
				Debug.LogError("Can not clone keyframe with " + value + ", " + time);
			}
			return new CustomKeyframe<T>(clonedValue!, time);
		}
	}
}
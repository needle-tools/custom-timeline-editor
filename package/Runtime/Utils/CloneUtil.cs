﻿#nullable enable
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace Needle.Timeline
{
	public class CouldNotCloneException : Exception
	{
	}

	public static class CloneUtil
	{
		public static T TryClone<T>(T value)
		{
			if (value is null)
			{
				return default!;
			}

			if (value?.GetType().IsValueType ?? false)
			{
				return value;
			}

			return (T)TryClone((object)value!);
		}

		public static object TryClone(object? value)
		{
			if (value is null)
			{
				return null!;
			}

			if (value.GetType().IsValueType)
			{
				return value;
			}

			if (value is ICloneable cloneable)
			{
				object res = cloneable.Clone();
				return res;
			}


			if (value is ComputeBuffer buffer)
			{
				var copy = new ComputeBuffer(buffer.count, buffer.stride);
				var arr = new byte[buffer.count * buffer.stride];
				buffer.GetData(arr);
				copy.SetData(arr);
				return copy;
			}

			if (value is IList col)
			{
				// TODO: check if list contains value types, otherwise we need to copy list content as well
				return Activator.CreateInstance(value.GetType(), col);
			}


			var newInstance = Activator.CreateInstance(value.GetType());
			if (newInstance != null)
			{
				if (TryCloneMembers(value, newInstance))
					return newInstance;
			}

			throw new CouldNotCloneException();
		}

		private static bool TryCloneMembers(object source, object target)
		{
			foreach (var field in source.GetType().GetFields(BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
			{
				Debug.Log(field.Name);
				var val = field.GetValue(source);
				field.SetValue(target, val);
			}
			return true;
		}
	}
}
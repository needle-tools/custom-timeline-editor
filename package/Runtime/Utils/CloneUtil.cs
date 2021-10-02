#nullable enable
using System;
using System.Collections;
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
			
			if (value?.GetType().IsValueType ?? false)
			{
				return value;
			}

			if (value is ICloneable cloneable)
			{
				object res = cloneable.Clone();
				return res;
			}

			if (value is IList col)
			{
				return Activator.CreateInstance(value.GetType(), col);
			}

			if (value is ComputeBuffer buffer)
			{
				var copy = new ComputeBuffer(buffer.count, buffer.stride);
				var arr = new byte[buffer.count * buffer.stride];
				buffer.GetData(arr);
				copy.SetData(arr);
				return copy;
			}

			throw new CouldNotCloneException();
		}
	}
}
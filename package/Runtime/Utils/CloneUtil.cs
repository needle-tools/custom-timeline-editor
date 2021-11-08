#nullable enable
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
				var clonedList = (IList)Activator.CreateInstance(value.GetType(), col);
				var didCheckType = false;
				for (var i = 0; i < clonedList.Count; i++)
				{
					var entry = clonedList[i];
					if (!didCheckType && entry != null)
					{
						didCheckType = true;
						var contentType = entry.GetType();
						if (!contentType.IsClass)
							break;
					}
					clonedList[i] = TryClone(entry);
				}
				return clonedList;
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
				var val = field.GetValue(source);
				var type = val?.GetType();
				if (type != null)
				{
					if (!type.IsValueType)
					{
						val = TryClone(val);
					}
				}
				field.SetValue(target, val);
			}
			return true;
		}
	}
}
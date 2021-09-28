#nullable enable
using System;
using System.Collections;

namespace Needle.Timeline
{
	public class CouldNotCloneException : Exception
	{
	}

	public static class CloneUtil
	{
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

			throw new CouldNotCloneException();
		}
	}
}
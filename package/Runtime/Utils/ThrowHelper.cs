#nullable enable

using System;

namespace Needle.Timeline
{
	public static class ThrowHelper
	{
		public static void Throw(string? message = null)
		{
			throw new Exception(message);
		}
		
		public static void Throw<T>(string? message = null) where T : Exception, new()
		{
			if (message != null)
				throw (T)Activator.CreateInstance(typeof(T), message);
			throw new T();
		}
	}
}
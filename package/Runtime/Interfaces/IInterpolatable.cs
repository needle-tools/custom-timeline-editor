using System;

namespace Needle.Timeline
{
	public interface IInterpolatable
	{
		void Interpolate(ref object instance, object obj0, object obj1, float t);
	}

	public interface IInterpolatable<T> : IInterpolatable
	{
		void Interpolate(ref T instance, T t0, T t1, float t);
	}

	public static class IInterpolateExtensions
	{
		public static void Cast<T>(this IInterpolatable<T> generic, ref object instance, object t0, object t1, float t)
		{
			var m = (T)instance;
			generic.Interpolate(ref m, (T)t0, (T)t1, t);
			instance = m;
		}
	}

	public abstract class Interpolatable<T> : IInterpolatable<T>
	{
		public bool CanInterpolate(Type type)
		{
			return typeof(T).IsAssignableFrom(type);
		}

		public void Interpolate(ref object instance, object obj0, object obj1, float t)
		{
			this.Cast(ref instance, obj0, obj1, t);
		}

		public abstract void Interpolate(ref T instance, T t0, T t1, float t);
	}
}
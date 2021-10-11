using System;

namespace Needle.Timeline.Interfaces
{
	public interface IInterpolatable
	{
		void Interpolate(ref IInterpolatable instance, IInterpolatable t0, IInterpolatable t1, float t);
	}

	public interface IInterpolatable<T> : IInterpolatable
	{
		void Interpolate(ref T instance, T t0, T t1, float t);
	}

	public static class IInterpolateExtensions
	{
		public static void Cast<T>(this IInterpolatable<T> generic, ref IInterpolatable instance, IInterpolatable t0, IInterpolatable t1, float t)
		{
			var m = (T)instance;
			generic.Interpolate(ref m, (T)t0, (T)t1, t);
			instance = (IInterpolatable)m;
		}
	}

	public abstract class Interpolatable<T> : IInterpolatable<T>
	{
		public void Interpolate(ref IInterpolatable instance, IInterpolatable t0, IInterpolatable t1, float t)
		{
			this.Cast(ref instance, t0, t1, t);
		}

		public abstract void Interpolate(ref T instance, T t0, T t1, float t);
	}
}
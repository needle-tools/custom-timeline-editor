using System;

namespace Needle.Timeline
{
	public interface IInterpolator
	{
		bool CanInterpolate(Type type);
		object Interpolate(object v0, object v1, float t);
	}
	
	public interface IInterpolator<T> : IInterpolator
	{
		T Interpolate(T v0, T v1, float t);
	}

	public interface IHasInterpolator
	{
		IInterpolator Interpolator { get; set; }
	}
}
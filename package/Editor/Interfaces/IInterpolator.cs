using System;

namespace Needle.Timeline
{
	public interface IInterpolator
	{
		bool CanInterpolate(Type type);
		object Interpolate(object v0, object v1, float t);
	}
	
	public interface IInterpolator<T>
	{
		T Interpolate(T v0, T v1, float t);
	}
}
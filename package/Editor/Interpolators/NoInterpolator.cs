using System;

namespace Needle.Timeline
{
	public class NoInterpolator : IInterpolator
	{
		public bool CanInterpolate(Type type)
		{
			return true;
		}

		public object Interpolate(object v0, object v1, float t)
		{
			if (t > 0.99999f) return v1;
			return v0;
		}
	}
}
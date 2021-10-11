using System;
using System.Collections.Generic;

namespace Needle.Timeline
{
	[NoAutoSelect]
	public class NoInterpolator : IInterpolator
	{
		public object Instance { get; set; }

		public bool CanInterpolate(Type type)
		{
			return true;
		}
		
		public object Interpolate(object v0, object v1, float t)
		{
			if (t > 0.99f) return v1;
			return v0;
		}
	}
}
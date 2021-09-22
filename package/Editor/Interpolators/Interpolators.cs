using System;
using Unity.Jobs;

namespace Needle.Timeline
{
	public static class Interpolators
	{
		public static bool TryFindInterpolator(AnimateAttribute attribute, Type type, out IInterpolator interpolator)
		{
			interpolator = null;
			return false;
		}
	}
}
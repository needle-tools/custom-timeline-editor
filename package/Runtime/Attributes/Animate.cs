using System;

namespace Needle.Timeline
{
	[AttributeUsage(AttributeTargets.Field)]
	public class AnimateAttribute : Attribute
	{
		public bool AllowInterpolation = true;
		public Type Interpolator;
	}
}
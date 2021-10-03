using System;

namespace Needle.Timeline
{
	public class AnimateAttribute : Attribute
	{
		public bool AllowInterpolation = true;
		public Type Interpolator;
	}
}
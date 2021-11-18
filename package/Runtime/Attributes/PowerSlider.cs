using System;

namespace Needle.Timeline
{
	[AttributeUsage(AttributeTargets.Field)]
	public class PowerSlider : Attribute
	{
		public readonly float min, max, power;

		public PowerSlider(float min, float max, float power)
		{
			this.min = min;
			this.max = max;
			this.power = power;
		}
	}
}
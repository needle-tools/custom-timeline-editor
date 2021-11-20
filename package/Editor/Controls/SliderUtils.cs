using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	public static class SliderUtils
	{
		public static void CheckOverflow<T, U>(this T slider, U currentValue) 
			where T : BaseSlider<U> where U : IComparable<U>
		{
			slider.RemoveFromClassList("overflow-min");
			slider.RemoveFromClassList("overflow-max");
			slider.RemoveFromClassList("overflow-min-high");
			slider.RemoveFromClassList("overflow-max-high");
			const float highOverflowFactor = 3;
			var value = (float)currentValue.Cast(typeof(float));
			var min = (float)slider.lowValue.Cast(typeof(float));
			var max = (float)slider.highValue.Cast(typeof(float));
			var range = Mathf.Abs(min - max); 
			if (value < min)
			{
				var overflow = min - value;
				if (overflow > highOverflowFactor * range)
					slider.AddToClassList("overflow-min-high");
				else
					slider.AddToClassList("overflow-min");
			}
			else if (value > max)
			{
				var overflow = value - max;
				if (overflow > highOverflowFactor * range)
					slider.AddToClassList("overflow-max-high");
				else
					slider.AddToClassList("overflow-max");
			}
		}

		public static float CalculatePowerValue(float value, float power, float min, float max)
		{
			if (float.IsNaN(value))
			{
				value = 1;
			}
			var val01 = value.Remap(min, max, 0, 1);
			var wasNegative = val01 < 0;
			val01 = Mathf.Pow(Mathf.Abs(val01), power);
			if (wasNegative) val01 *= -1;
			value = val01.Remap(0, 1, min, max);
			return value;
		}

		public static float CalculatePowerValueInverse(float value, float power, float min, float max)
		{
			return CalculatePowerValue(value, 1 / power, min, max);
		}
	}
}
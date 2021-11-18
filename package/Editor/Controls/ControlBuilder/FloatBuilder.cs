using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	public class FloatBuilder : IControlBuilder
	{
		public bool CanBuild(Type type)
		{
			return typeof(float).IsAssignableFrom(type);
		}

		public VisualElement Build(Type type, IViewValueHandler viewValue, IContext? context = null)
		{
			var view = new FloatField();
			view.label = "_";
			view.RegisterValueChangedCallback(evt => { viewValue.SetValue(evt.newValue); });
			view.RegisterValueChangedCallback(viewValue);

			var val = viewValue.GetValue();
			if (val != null)
				view.value = (float)val;

			var range = context?.Attributes?.GetCustomAttribute<RangeAttribute>();
			if (range != null)
			{
				return Utils.MakeComposite(view, BuildSlider(range.min, range.max, viewValue, view));
			}
			var powerSlider = context?.Attributes?.GetCustomAttribute<PowerSlider>();
			if (powerSlider != null)
			{
				return Utils.MakeComposite(view, BuildSlider(powerSlider.min, powerSlider.max, viewValue, view, powerSlider.power));
			}
			

			return view;
		}

		private static VisualElement BuildSlider(float min, float max, 
			IValueProvider viewValue, FloatField view, float power = 1)
		{
			var isPowerSlider = Math.Abs(power - 1) > 0.01f && power != 0;
			var slider = new Slider(min, max);
			var value = viewValue.GetValue();
			if (value != null)
			{
				var _val = (float)value;
				if(isPowerSlider)
					_val = CalculatePowerValueInverse(_val, power, min, max);
				slider.value = _val;
			}
			slider.RegisterValueChangedCallback(evt =>
			{
				var _val = evt.newValue;
				if(isPowerSlider)
					_val = CalculatePowerValue(_val, power, min, max);
				view.value = _val;
				CheckSliderState( slider, _val, min, max);
			});
			view.RegisterValueChangedCallback(evt =>
			{
				var _val = evt.newValue;
				if(isPowerSlider)
					_val = CalculatePowerValueInverse(_val, power, min, max);
				slider.SetValueWithoutNotify(_val);
				CheckSliderState(slider, _val, min, max); 
			});
			return slider;
		}

		private static void CheckSliderState(VisualElement slider, float value, float min, float max)
		{
			if (value < min) 
				slider.AddToClassList("overflow-min");
			else if (value > max) 
				slider.AddToClassList("overflow-max");
			else
			{
				slider.RemoveFromClassList("overflow-min");
				slider.RemoveFromClassList("overflow-max");
			}
		}

		public static float CalculatePowerValue(float value, float power, float min, float max)
		{
			var val01 = value.Remap(min, max, 0, 1);
			val01 = Mathf.Pow(val01, power);
			value = val01.Remap(0, 1, min, max); 
			return value;
		}

		public static float CalculatePowerValueInverse(float value, float power, float min, float max)
		{
			return CalculatePowerValue(value, 1 / power, min, max);
		}
	}
}
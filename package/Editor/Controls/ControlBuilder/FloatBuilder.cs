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

		private static VisualElement BuildSlider(float min, float max, IViewValueHandler viewValue,
			FloatField view,
			float power = 1)
		{
			var isPowerSlider = Math.Abs(power - 1) > 0.01f && power != 0;
			var slider = new Slider(min, max);
			slider.tooltip = "From " + min.ToString("0.000") + " to " + max.ToString("0.000");
			var value = viewValue.GetValue();
			if (value != null)
			{
				var _val = (float)value;
				if (isPowerSlider)
					_val = SliderUtils.CalculatePowerValueInverse(_val, power, min, max);
				slider.value = _val;
				slider.CheckOverflow(_val);
			}
			slider.RegisterValueChangedCallback(evt =>
			{
				var _val = evt.newValue;
				if (isPowerSlider)
					_val = SliderUtils.CalculatePowerValue(_val, power, min, max);
				view.value = _val;
				slider.CheckOverflow(_val);
			});
			view.RegisterValueChangedCallback(evt =>
			{
				var _val = evt.newValue;
				if (isPowerSlider)
					_val = SliderUtils.CalculatePowerValueInverse(_val, power, min, max);
				slider.SetValueWithoutNotify(_val);
				slider.CheckOverflow(_val);
			});
			return slider;
		}
	}
}
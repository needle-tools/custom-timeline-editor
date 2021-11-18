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
				var slider = new Slider(range.min, range.max);
				var value = viewValue.GetValue();
				if (value != null)
					slider.value = (float)value;
				slider.RegisterValueChangedCallback(evt =>
				{
					var _val = evt.newValue;
					_val = CalculatePowerValue(_val, 2, range.min, range.max);
					view.value = _val;
					CheckSliderState();
				});
				view.RegisterValueChangedCallback(evt =>
				{
					var _val = evt.newValue;
					_val = CalculatePowerValueInverse(_val, 2f, range.min, range.max);
					slider.SetValueWithoutNotify(_val);
					CheckSliderState(); 
				});

				void CheckSliderState()
				{
					if (view.value < range.min) 
						slider.AddToClassList("overflow-min");
					else if (view.value > range.max) 
						slider.AddToClassList("overflow-max");
					else
					{
						slider.RemoveFromClassList("overflow-min");
						slider.RemoveFromClassList("overflow-max");
					}
				}

				return Utils.MakeComposite(view, slider);
			}

			return view;
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
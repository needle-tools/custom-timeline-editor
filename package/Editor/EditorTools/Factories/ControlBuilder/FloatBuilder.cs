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
			var val = viewValue.GetValue();
			if(val != null)
				view.value = (float)val;

			var range = context?.Attributes?.GetCustomAttribute<RangeAttribute>();
			if (range != null) 
			{
				var slider = new Slider(range.min, range.max);
				var value = viewValue.GetValue();
				if(value != null)
					slider.value = (float)value;
				slider.RegisterValueChangedCallback(evt =>
				{
					view.SetValueWithoutNotify(evt.newValue);
					viewValue.SetValue(evt.newValue);
				});
				view.RegisterValueChangedCallback(evt =>
				{
					view.value = Mathf.Clamp(view.value, range.min, range.max);
					slider.SetValueWithoutNotify(view.value);
				});
				
				return Utils.MakeComposite(view, slider);
			}

			return view;
		}
	}
}
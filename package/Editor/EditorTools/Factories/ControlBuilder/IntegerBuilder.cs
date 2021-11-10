using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	public class IntegerBuilder : IControlBuilder
	{
		public bool CanBuild(Type type)
		{
			return typeof(int).IsAssignableFrom(type);
		}

		public VisualElement Build(Type type, IViewValueHandler viewValue, IContext context)
		{
			var view = new IntegerField();
			view.RegisterValueChangedCallback(evt => { viewValue.SetValue(evt.newValue); });
			var val = viewValue.GetValue();
			if(val != null)
				view.value = (int)val;
			view.label = "_";

			if (context?.Attributes != null) 
			{
				var range = context.Attributes.GetCustomAttribute<RangeAttribute>();
				if (range != null) 
				{
					var sliderContainer = new VisualElement();
					sliderContainer.AddToClassList("control");
					// sliderContainer.AddToClassList("one-row");
					var slider = new SliderInt((int)range.min, (int)range.max);
					slider.value = (int)viewValue.GetValue();
					slider.RegisterValueChangedCallback(evt =>
					{
						view.SetValueWithoutNotify(evt.newValue);
						viewValue.SetValue(evt.newValue);
					});
					view.RegisterValueChangedCallback(evt => { slider.SetValueWithoutNotify(evt.newValue); });

					sliderContainer.Add(view);
					sliderContainer.Add(slider);
					return sliderContainer;
				}
			}
			return view;
		}
	}
}
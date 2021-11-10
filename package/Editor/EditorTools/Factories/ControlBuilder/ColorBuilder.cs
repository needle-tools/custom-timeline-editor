using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	public class ColorBuilder : IControlBuilder
	{
		public bool CanBuild(Type type)
		{
			return typeof(Color).IsAssignableFrom(type);
		}

		public VisualElement? Build(Type type, IViewValueHandler viewValue, IContext? context = null)
		{
			var view = new ColorField();
			view.label = "_";
			view.RegisterValueChangedCallback(evt => { viewValue.SetValue(evt.newValue); });
			var val = viewValue.GetValue();
			if(val != null)
				view.value = (Color)val; 
			else
			{
				view.value = Color.white;
				viewValue.SetValue(view.value);
			}
			var usage = context?.Attributes?.GetCustomAttribute<ColorUsageAttribute>();
			if (usage != null)
			{
				view.hdr = usage.hdr;
				view.showAlpha = usage.showAlpha;
			}
			return view;
		}
	}
}
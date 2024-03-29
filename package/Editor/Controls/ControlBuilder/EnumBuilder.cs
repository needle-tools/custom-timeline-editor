﻿using System;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	public class EnumBuilder : IControlBuilder
	{
		public bool CanBuild(Type type)
		{
			return typeof(Enum).IsAssignableFrom(type);
		}

		public VisualElement Build(Type type, IViewValueHandler viewValue, IContext context = null)
		{
			var enumOptions = Enum.GetNames(type);
			var view = new PopupField<string>(enumOptions.ToList(), 0);
			view.label = "_";

			view.RegisterValueChangedCallback(evt =>
			{
				var val = (Enum)Enum.Parse(type, evt.newValue);
				viewValue.SetValue(val);
			});
			
			var val = viewValue.GetValue();
			if(val != null)
				view.value = val.ToString();
			
			viewValue.ValueChanged += newValue =>
			{
				view.SetValueWithoutNotify(newValue.ToString());
			};

			return view;
		}
	}
}
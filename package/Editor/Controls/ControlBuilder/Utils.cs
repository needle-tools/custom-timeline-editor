﻿using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	internal static class Utils
	{
		public static VisualElement MakeComposite(VisualElement pre, VisualElement main)
		{
			var composite = new VisualElement();
			composite.AddToClassList("composite-control");
			composite.Add(pre);
			composite.Add(main);
			pre.AddToClassList("pre");
			main.AddToClassList("main");
			return composite;
		}

		internal static void RegisterValueChangedCallback<T>(this BaseField<T> field, IViewValueHandler viewValue)
		{
			viewValue.ValueChanged += val => field.value = (T)val;
		}
	}
}
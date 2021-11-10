using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	public abstract class SimpleGenericBuilder<T> : IControlBuilder
	{
		public bool CanBuild(Type type)
		{
			return typeof(T).IsAssignableFrom(type);
		}

		public VisualElement? Build(Type type, IViewValueHandler viewValue, IContext? context = null)
		{
			var view = GetField();
			view.label = "_";
			view.RegisterValueChangedCallback(evt => { viewValue.SetValue(evt.newValue); });
			var val = viewValue.GetValue();
			if(val != null)
				view.value = (T)val;
			return view;
		}

		protected abstract BaseField<T> GetField();
	}
}
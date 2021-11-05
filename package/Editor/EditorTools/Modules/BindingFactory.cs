using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	internal static class BindingFactory
	{
		public static bool TryProduceBinding(ModuleView view, FieldInfo field, ToolTarget target, IBindsFields bindable, out BindingHandler res)
		{
			if (field.IsStatic)
			{
				res = null;
				return false;
			}
			var binding = new BindingHandler(view, target.Clip as IRecordable, field, new ViewValue());
			res = binding; 
			CreateFieldView(field, binding);
			
			if (binding.VisualElement == null)
			{
				Debug.LogWarning("Did not find handler for " + field.FieldType);
				return false;
			}
			
			binding.Enabled = true;
			bindable.Bindings.Add(binding);
			return true;
		}

		private static void CreateFieldView(FieldInfo field, BindingHandler binding)
		{
			
			if (typeof(Enum).IsAssignableFrom(field.FieldType))
			{
				var choices = new List<string>();
				var enumOptions = Enum.GetNames(field.FieldType);
				for (var i = 0; i < enumOptions.Length; i++) choices.Add(enumOptions[i]);
				
				var view = new PopupField<string>(choices, 0);
				view.label = field.Name;

				view.RegisterValueChangedCallback(evt =>
				{
					var val = (Enum)Enum.Parse(field.FieldType, evt.newValue);
					binding.View.SetValue(val);
				});
				var val = (Enum)Enum.Parse(field.FieldType, view.value);
				binding.View.SetValue(val);
				binding.VisualElement = view;
			}
			if (typeof(Vector3).IsAssignableFrom(field.FieldType))
			{
				var view = new Vector3Field(field.Name);
				view.RegisterValueChangedCallback(evt => { binding.View.SetValue(evt.newValue); });
				binding.View.SetValue(view.value);
				binding.VisualElement = view;
			}
			else if (typeof(Color).IsAssignableFrom(field.FieldType))
			{
				var view = new ColorField(field.Name);
				view.value = Color.white;
				view.RegisterValueChangedCallback(evt => { binding.View.SetValue(evt.newValue); });
				binding.View.SetValue(view.value);
				binding.VisualElement = view;
			}

			if (binding.VisualElement != null)
			{
				var controls = binding.VisualElement;
				var layout = new VisualElement();
				layout.style.flexDirection = FlexDirection.Row;
				var toggle = new Toggle();
				layout.Add(toggle);
				layout.Add(binding.VisualElement);
				binding.VisualElement = layout;
				toggle.RegisterValueChangedCallback(evt => binding.Enabled = evt.newValue);
				binding.EnabledChanged += () =>
				{
					toggle.SetValueWithoutNotify(binding.Enabled);
					controls.SetEnabled(binding.Enabled);
				};
				controls.SetEnabled(binding.Enabled);
			}
		}

		// private static Action BindDynamicValueChangedEvent()
		// {
		// 	Expression.MemberBind()
		// 	var expr = Expression.Invoke()
		// }

		// private static LambdaExpression CreateLambda(Type type)
		// {
		// 	var genericType = typeof(ChangeEvent<>).MakeGenericType(type);
		// 	var callback = typeof(EventCallback<>).MakeGenericType(genericType);
		// 	var call = Expression.Call(typeof(CallbackEventHandler), "RegisterCallback", new[] { genericType }, source);
		//
		// 	// return Expression.Lambda(call, source);
		// 	return null;
		// }

		private class MyEvent : EventBase<MyEvent>, INotifyValueChanged<object>
		{
			public void SetValueWithoutNotify(object newValue)
			{
				
			}

			public object value { get; set; }
		}
	}
}
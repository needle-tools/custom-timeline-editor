using System.Reflection;

namespace Needle.Timeline
{
	internal static class BindingFactory
	{
		public static bool TryProduceBinding(ModuleViewController viewController, FieldInfo field, ToolTarget target, IBindsFields bindable, out ViewFieldBindingController res)
		{
			if (field.IsStatic)
			{
				res = null;
				return false;
			}

			PersistenceHelper.TryGetPreviousValue(field, out var currentValue);
			res = new ViewFieldBindingController(target.Clip, field, new ViewValueProxy(currentValue));
			res.ViewElement = res.BuildControl();
			bindable.Bindings.Add(res);
			// res = binding; 
			// CreateFieldView(field, binding);
			//
			// if (binding.ViewElement == null)
			// {
			// 	Debug.LogWarning("Did not find handler for " + field.FieldType);
			// 	return false;
			// }
			//
			// binding.Enabled = true;
			// bindable.Bindings.Add(binding);
			return res.ViewElement != null;
		}

		// private static void CreateFieldView(FieldInfo field, IViewFieldBinding binding)
		// {
		// 	if (typeof(Enum).IsAssignableFrom(field.FieldType))
		// 	{
		// 		var choices = new List<string>();
		// 		var enumOptions = Enum.GetNames(field.FieldType);
		// 		for (var i = 0; i < enumOptions.Length; i++) choices.Add(enumOptions[i]);
		// 		
		// 		var view = new PopupField<string>(choices, 0);
		// 		view.label = field.Name;
		//
		// 		view.RegisterValueChangedCallback(evt =>
		// 		{
		// 			var val = (Enum)Enum.Parse(field.FieldType, evt.newValue);
		// 			binding.ViewValue.SetValue(val);
		// 		});
		// 		var val = (Enum)Enum.Parse(field.FieldType, view.value);
		// 		binding.ViewValue.SetValue(val);
		// 		binding.ViewElement = view;
		// 	}
		// 	if (typeof(Vector3).IsAssignableFrom(field.FieldType))
		// 	{
		// 		var view = new Vector3Field(field.Name);
		// 		view.RegisterValueChangedCallback(evt => { binding.ViewValue.SetValue(evt.newValue); });
		// 		binding.ViewValue.SetValue(view.value);
		// 		binding.ViewElement = view;
		// 	}
		// 	if (typeof(float).IsAssignableFrom(field.FieldType))
		// 	{
		// 		var view = new FloatField(field.Name);
		// 		view.RegisterValueChangedCallback(evt => { binding.ViewValue.SetValue(evt.newValue); });
		// 		binding.ViewValue.SetValue(view.value);
		// 		binding.ViewElement = view;
		// 	}
		// 	else if (typeof(Color).IsAssignableFrom(field.FieldType))
		// 	{
		// 		var view = new ColorField(field.Name);
		// 		view.value = Color.white;
		// 		view.RegisterValueChangedCallback(evt => { binding.ViewValue.SetValue(evt.newValue); });
		// 		binding.ViewValue.SetValue(view.value);
		// 		binding.ViewElement = view;
		// 	}
		//
		// 	if (binding.ViewElement != null)
		// 	{
		// 		var controls = binding.ViewElement;
		// 		var layout = new VisualElement();
		// 		layout.style.flexDirection = FlexDirection.Row;
		// 		var toggle = new Toggle();
		// 		layout.Add(toggle);
		// 		layout.Add(binding.ViewElement);
		// 		binding.ViewElement = layout;
		// 		toggle.RegisterValueChangedCallback(evt => binding.Enabled = evt.newValue);
		// 		binding.EnabledChanged += val =>
		// 		{
		// 			toggle.SetValueWithoutNotify(val);
		// 			controls.SetEnabled(val);
		// 		};
		// 		controls.SetEnabled(binding.Enabled);
		// 	}
		// } 
		//
		// // private static Action BindDynamicValueChangedEvent()
		// // {
		// // 	Expression.MemberBind()
		// // 	var expr = Expression.Invoke()
		// // }
		//
		// // private static LambdaExpression CreateLambda(Type type)
		// // {
		// // 	var genericType = typeof(ChangeEvent<>).MakeGenericType(type);
		// // 	var callback = typeof(EventCallback<>).MakeGenericType(genericType);
		// // 	var call = Expression.Call(typeof(CallbackEventHandler), "RegisterCallback", new[] { genericType }, source);
		// //
		// // 	// return Expression.Lambda(call, source);
		// // 	return null;
		// // }
		//
		// private class MyEvent : EventBase<MyEvent>, INotifyValueChanged<object>
		// {
		// 	public void SetValueWithoutNotify(object newValue)
		// 	{
		// 		
		// 	}
		//
		// 	public object value { get; set; }
		// }
	}
}
using System;
using System.Reflection;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	public class FieldViewBinding : IViewFieldBinding, IViewValueHandler
	{
		public bool Enabled
		{
			get => enabled;
			set
			{
				if (value == enabled) return;
				enabled = value;
				EnabledChanged?.Invoke(enabled);
			}
		}
		public event Action<bool> EnabledChanged;
		
		public IViewValueHandler ViewValue => this;
		public VisualElement ViewElement { get; set; }
		
		
		public object GetValue(object instance)
		{
			return field.GetValue(instance);
		}

		public void SetValue(object instance, object value)
		{
			field.SetValue(instance, value);
		}

		public Type ValueType => field.FieldType;
		public string Name => field.Name;
		
		public T GetCustomAttribute<T>() where T : Attribute
		{
			return field.GetCustomAttribute<T>();
		}

		public bool Equals(MemberInfo member)
		{
			return field.Equals(member);
		}

		public object GetValue()
		{
			return viewValue;
		}

		public void SetValue(object newValue)
		{
			if (viewValue == newValue) return;
			viewValue = newValue;
			ViewValueChanged?.Invoke(viewValue);
		}

		public event Action<object> ViewValueChanged;

		public FieldViewBinding(FieldInfo field, object value = null)
		{
			this.field = field;
			this.viewValue = value;
		}

		private object viewValue;
		private FieldInfo field;
		private bool enabled;
	}
}
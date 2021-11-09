using System;
using System.Linq;
using System.Reflection;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	internal class MockBinding : IViewFieldBinding, IViewValueHandler
	{
		private readonly string name;
		private object value;
		private readonly Attribute[] customAttributes;
		private readonly Type type;

		public MockBinding(string name, object value, bool enabled = true, params Attribute[] customAttributes)
		{
			this.name = name;
			this.value = value;
			this.customAttributes = customAttributes;
			this.type = value.GetType();
			this.Enabled = enabled;
		}
			
		public bool Enabled { get; set; }
		public event Action<bool> EnabledChanged;
		public IViewValueHandler ViewValue => this;
		public VisualElement ViewElement { get; set; }
			
		public object GetValue(object instance)
		{
			return value;
		}

		public void SetValue(object instance, object value)
		{
			this.value = value;
		}

		public Type ValueType => type;
		public string Name => name;
		public T GetCustomAttribute<T>() where T : Attribute => customAttributes.FirstOrDefault(a => a is T) as T;
		public bool Equals(MemberInfo member)
		{
			return member is FieldInfo f && f.FieldType == type && f.Name == name;
		}

		public object GetValue()
		{
			return value;
		}

		public void SetValue(object newValue)
		{
			if (newValue == value) return;
			this.value = newValue;
			ValueChanged?.Invoke(value);
		}

		public void SetValueWithoutNotify(object newValue)
		{
			this.value = newValue;
		}

		public event Action<object> ValueChanged;
	}
}
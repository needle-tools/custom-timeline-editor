﻿using System;
using System.Linq;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	internal class MockBinding : IViewFieldBinding, IValueHandler
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
		public event Action EnabledChanged;
		public IValueHandler ViewValue => this;
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

		public object GetValue()
		{
			return value;
		}

		public void SetValue(object newValue)
		{
			this.value = newValue;
		}
	}
}
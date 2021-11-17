using System;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	public interface IValueProvider
	{
		object GetValue();
	}
	
	public interface IValueHandler : IValueProvider
	{
		void SetValue(object newValue);
	}

	public interface IViewValueHandler : IValueHandler
	{
		string Name { get; }
		Type ValueType { get; }
		void SetValueWithoutNotify(object newValue);
		event Action<object> ValueChanged;
	}

	public interface IHasCustomAttributes
	{
		T GetCustomAttribute<T>() where T : Attribute;
	}

	public interface IViewFieldBinding : IEnabled, IHasCustomAttributes
	{
		IViewValueHandler ViewValue { get; }
		VisualElement ViewElement { get; set; }
		object GetValue(object instance);
		void SetValue(object instance, object value);
		Type ValueType { get; }
		string Name { get; }
		bool Equals(MemberInfo member);
		bool Matches(MemberInfo member);
		void SetField(FieldInfo field);
	}
}
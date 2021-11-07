using System;
using System.Reflection;
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
	
	public interface IInstanceValueHandler : IValueHandler
	{
		object Instance { get; set; }
	}

	public interface IViewValueHandler : IValueHandler
	{
		event Action<object> ViewValueChanged;
	}

	public interface IViewFieldBinding : IEnabled
	{
		IValueHandler ViewValue { get; }
		VisualElement ViewElement { get; set; }
		object GetValue(object instance);
		void SetValue(object instance, object value);
		Type ValueType { get; }
		string Name { get; }
		T GetCustomAttribute<T>() where T : Attribute;
		bool Equals(MemberInfo member);
	}
}
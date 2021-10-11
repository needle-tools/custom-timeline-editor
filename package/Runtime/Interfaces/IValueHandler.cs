using System;

namespace Editor
{
	public interface IValueHandler
	{
		void SetValue(object value);
		object GetValue();
	}
}
using System;

namespace Needle.Timeline
{
	public class ViewValueProxy : IViewValueHandler
	{
		private object _value;

		public ViewValueProxy(object value) => this._value = value;

		public object GetValue()
		{
			return _value;
		}

		public void SetValue(object newValue)
		{
			if(newValue == _value) return;
			_value = newValue;
			ValueChanged?.Invoke(_value);
		}

		public void SetValueWithoutNotify(object newValue)
		{
			_value = newValue;
		}

		public event Action<object> ValueChanged;
	}
}
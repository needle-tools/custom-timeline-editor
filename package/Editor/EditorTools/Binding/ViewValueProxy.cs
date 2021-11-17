using System;

namespace Needle.Timeline
{
	public class ViewValueProxy : IViewValueHandler
	{
		private string _name;
		private object _value;
		private Type _type;

		public ViewValueProxy(string name, object value)
		{
			this._name = name;
			this._value = value;
			if (_value != null)
				this._type = _value.GetType();
		}

		public object GetValue()
		{
			return _value;
		}

		public void SetValue(object newValue)
		{
			if (newValue == _value) return;
			_value = newValue;
			if (_value != null)
				this._type = _value.GetType();
			ValueChanged?.Invoke(_value);
		}

		public string Name => _name;
		public Type ValueType => _type;

		public void SetValueWithoutNotify(object newValue)
		{
			_value = newValue;
		}

		public event Action<object> ValueChanged;
	}
}
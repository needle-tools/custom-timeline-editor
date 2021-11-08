using System;

namespace Needle.Timeline.Tests
{
	
	internal struct ValueHandler : IViewValueHandler
	{
		private object value;

		internal ValueHandler(object val)
		{
			value = val;
			ViewValueChanged = null;
		}
		
		public object GetValue()
		{
			return value;
		}

		public void SetValue(object newValue)
		{
			if (value == newValue) return;
			value = newValue;
			ViewValueChanged?.Invoke(value);
		}

		public event Action<object> ViewValueChanged;
	}
}
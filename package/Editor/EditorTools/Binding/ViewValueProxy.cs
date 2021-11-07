namespace Needle.Timeline
{
	internal class ViewValueProxy : IValueHandler
	{
		private object _value;

		public ViewValueProxy(object value) => this._value = value;

		public object GetValue()
		{
			return _value;
		}

		public void SetValue(object newValue)
		{
			_value = newValue;
		}
	}
}
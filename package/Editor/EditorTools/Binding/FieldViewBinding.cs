using System;
using System.Reflection;

namespace Needle.Timeline
{
	public class FieldViewBinding : IViewValueHandler
	{
		public object GetValue()
		{
			return field.GetValue(instance);
		}

		public void SetValue(object newValue)
		{
			var cur = GetValue();
			if (cur == null && newValue == null) return;

			if (newValue != null && newValue.GetType() != field.FieldType)
			{
				newValue = newValue.Cast(field.FieldType);
			}
			
			if (cur != null && cur.Equals(newValue)) return;
			if (newValue != null && newValue.Equals(cur)) return;
			
			
			field.SetValue(instance, newValue);
			ValueChanged?.Invoke(newValue);
		}

		public void SetValueWithoutNotify(object newValue)
		{
			field.SetValue(instance, newValue);
		}

		public event Action<object> ValueChanged;

		public FieldViewBinding(object instance, FieldInfo field)
		{
			this.instance = instance;
			this.field = field;
		}

		private readonly object instance;
		private readonly FieldInfo field;
	}
}
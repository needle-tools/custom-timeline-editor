using System;
using System.Reflection;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	internal class ViewValue : IValueHandler
	{
		private object _value;

		public object GetValue()
		{
			return _value;
		}

		public void SetValue(object newValue)
		{
			_value = newValue;
		}
	}

	internal class BindingHandler : IViewFieldBinding
	{
		public IValueHandler View { get; }

		public bool Enabled
		{
			get => enabled && (rec?.IsRecording ?? true);
			set
			{
				if (value == enabled) return;
				enabled = value;
				EnabledChanged?.Invoke();
			}
		}

		public event Action EnabledChanged;

		internal VisualElement VisualElement;

		private readonly FieldInfo field;
		private bool enabled = false;
		private readonly ModuleView module;
		private IRecordable rec;

		public BindingHandler(ModuleView module, IRecordable rec, FieldInfo field, IValueHandler view)
		{
			this.module = module;
			this.rec = rec;
			this.field = field;
			View = view;
		}

		public object GetValue(object instance)
		{
			return field.GetValue(instance);
		}

		public void SetValue(object instance, object value)
		{
			if (!field.IsStatic && instance == null || instance?.GetType() != field.DeclaringType) return;
			field.SetValue(instance, value);
		}
	}
}
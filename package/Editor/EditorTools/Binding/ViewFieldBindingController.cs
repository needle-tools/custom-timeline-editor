using System;
using System.Reflection;
using UnityEngine.Assertions.Must;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	internal class ViewFieldBindingController : IViewFieldBinding
	{
		public IValueHandler ViewValue { get; }
		public VisualElement ViewElement { get; set; }

		public bool Enabled
		{
			get => enabled && (rec?.IsRecording ?? true);
			set
			{
				if (value == enabled) return;
				enabled = value;
				EnabledChanged?.Invoke(enabled);
			}
		}

		public event Action<bool> EnabledChanged;

		public object GetValue(object instance)
		{
			return field.GetValue(instance);
		}

		public void SetValue(object instance, object value)
		{
			if (!field.IsStatic && instance == null || instance?.GetType() != field.DeclaringType) return;
			field.SetValue(instance, value);
		}

		public Type ValueType => field.FieldType;
		public string Name => field.Name;
		public T GetCustomAttribute<T>() where T : Attribute => field.GetCustomAttribute<T>();


		private readonly FieldInfo field;
		private readonly IRecordable rec;
		
		private bool enabled = false;

		public ViewFieldBindingController(IRecordable rec, FieldInfo field, IValueHandler view)
		{
			this.rec = rec;
			this.field = field;
			ViewValue = view;
			this.rec.RecordingStateChanged += OnRecordingChanged;
		}

		internal void Init()
		{
			OnRecordingChanged(this.rec.IsRecording);
		}

		private void OnRecordingChanged(bool obj)
		{
			ViewElement.style.display = obj ? DisplayStyle.Flex : DisplayStyle.None;
		}
	}
}
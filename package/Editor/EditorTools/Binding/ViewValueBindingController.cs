#nullable enable

using System;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine.Assertions.Must;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	internal class ViewValueBindingController : IViewFieldBinding
	{
		public IViewValueHandler ViewValue { get; }
		public VisualElement? ViewElement { get; set; }

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

		public event Action<bool>? EnabledChanged;

		public object GetValue(object instance)
		{
			return field.GetValue(instance);
		}

		public void SetValue(object? instance, object value)
		{
			if (!field.IsStatic && instance == null || instance?.GetType() != field.DeclaringType) return;
			field.SetValue(instance, value);
		}

		public Type ValueType => field.FieldType;
		public string Name => field.Name;
		public T GetCustomAttribute<T>() where T : Attribute => field.GetCustomAttribute<T>();

		public bool Equals(MemberInfo member)
		{
			return field.Equals(member);
		}


		private readonly FieldInfo field;
		private readonly IRecordable? rec;
		private bool enabled = false;

		public ViewValueBindingController(FieldInfo field, IViewValueHandler view, IRecordable? rec)
		{
			this.field = field;
			ViewValue = view;
			this.rec = rec;
			if (rec != null)
				rec.RecordingStateChanged += OnRecordingChanged;
		}

		internal void Init()
		{
			if (rec != null)
				OnRecordingChanged(rec.IsRecording);
		}

		private void OnRecordingChanged(bool obj)
		{
			ViewElement?.SetEnabled(obj);
		}
	}
}
using System;

namespace Needle.Timeline
{
	public interface ICustomKeyframe
	{
		object value { get; set; }
		float time { get; set; }
		event Action TimeChanged, ValueChanged;
	}
	
	public interface ICustomKeyframe<T> : ICustomKeyframe
	{
		new T value { get; set; }
	}

	public class CustomKeyframe<T> : ICustomKeyframe<T>
	{
		private float time1;
		private T value1;

		object ICustomKeyframe.value
		{
			get => value;
			set
			{
				if(value.GetType() == typeof(T))
					this.value = (T)value;
			}
		}

		public T value
		{
			get => value1;
			set
			{
				if (value == null && value1 == null || (value?.Equals(value1) ?? false)) return;
				value1 = value;
				ValueChanged?.Invoke();
			}
		}

		public float time
		{
			get => time1;
			set
			{
				if (Math.Abs(value - time1) < 0.00000001f) return;
				time1 = value;
				TimeChanged?.Invoke();
			}
		}

		public event Action TimeChanged;
		public event Action ValueChanged;

		public CustomKeyframe(T value, float time)
		{
			this.value = value;
			this.time = time;
		}
	}
}
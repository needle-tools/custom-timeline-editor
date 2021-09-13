namespace Needle.Timeline
{
	public interface ICustomKeyframe
	{
		object value { get; set; }
		float time { get; set; }
	}
	
	public interface ICustomKeyframe<T> : ICustomKeyframe
	{
		new T value { get; set; }
	}

	public class CustomKeyframe<T> : ICustomKeyframe<T>
	{
		object ICustomKeyframe.value
		{
			get => value;
			set
			{
				if(value.GetType() == typeof(T))
					this.value = (T)value;
			}
		}

		public T value { get; set; }
		public float time { get; set; }

		public CustomKeyframe(T value, float time)
		{
			this.value = value;
			this.time = time;
		}
	}
}
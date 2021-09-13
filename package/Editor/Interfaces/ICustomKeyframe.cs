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
}
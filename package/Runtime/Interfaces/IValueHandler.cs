namespace Needle.Timeline
{
	public interface IValueProvider
	{
		object GetValue();
	}
	
	public interface IValueHandler : IValueProvider
	{
		void SetValue(object newValue);
	}
	
	public interface IInstanceValueHandler : IValueHandler
	{
		object Instance { get; set; }
	}
}
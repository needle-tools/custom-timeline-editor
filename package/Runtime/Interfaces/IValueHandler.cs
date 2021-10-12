namespace Needle.Timeline
{
	public interface IValueHandler
	{
		void SetValue(object value);
		object GetValue();
	}
	
	public interface IInstanceValueHandler : IValueHandler
	{
		object Instance { get; set; }
	}
}
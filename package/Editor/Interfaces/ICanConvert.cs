namespace Needle.Timeline.Interfaces
{
	public interface ICanConvert<in TInput, out TOutput>
	{
		TOutput Convert(TInput input);
	}
}
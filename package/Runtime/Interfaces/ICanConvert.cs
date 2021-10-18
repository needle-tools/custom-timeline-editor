namespace Needle.Timeline
{
	public interface ICanConvert<in TInput, out TOutput>
	{
		TOutput Convert(TInput input);
	}
}
using Needle.Timeline.Serialization;

namespace Needle.Timeline
{
	public static class DefaultSerializer
	{
		public static ISerializer Get { get; } = new NewtonsoftSerializer();
	}
}
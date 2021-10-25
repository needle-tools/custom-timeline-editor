namespace Needle.Timeline.ResourceProviders
{
	public static class DefaultResources
	{
		private static IComputeBufferProvider _cb;
		public static IComputeBufferProvider GlobalComputeBufferProvider => _cb ??= new DefaultComputeBufferProvider();
	}
}
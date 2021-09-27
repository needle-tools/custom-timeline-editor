using System;

namespace Needle.Timeline
{
	public static class DisposeUtils
	{
		public static void SafeDispose(this IDisposable d)
		{
			d?.Dispose();
		}
	}
}
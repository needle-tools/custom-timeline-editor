using Needle.Timeline.ResourceProviders;

namespace Needle.Timeline.Tests.ResourceTests
{
	public class ResourceTestBase
	{
		public IResourceProvider Resources
		{
			get
			{
				var res = ResourceProvider.CreateDefault();
				res.ClearCaches();
				return res;
			}
		}
	}
}
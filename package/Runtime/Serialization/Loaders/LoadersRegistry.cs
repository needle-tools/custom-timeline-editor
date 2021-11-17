#nullable enable
using System.Collections.Generic;
using Needle.Timeline.Serialization;

namespace Needle.Timeline
{
	public class LoadersRegistry : ImplementorsRegistry<ILoader>
	{
		private static readonly LoadersRegistry _instance = new LoadersRegistry();
		private static readonly IList<IArgument> _args = new List<IArgument>
		{
			DefaultSerializer.Get.AsArg(),
		};
		
		public static ILoader? GetDefault()
		{ 
			if (_instance.TryGetInstance(l => l != null, out var i, _args))
				return i;
			return null;
		}
	}
}
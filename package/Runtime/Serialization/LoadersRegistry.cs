using System.Collections.Generic;
using Needle.Timeline.Serialization;

namespace Needle.Timeline
{
	public class LoadersRegistry : ImplementorsRegistry<ILoader>
	{
		private static readonly LoadersRegistry _instance = new LoadersRegistry();
		private static readonly IList<IArgument> _args = new List<IArgument>() { new Argument(null, new JsonSerializer()) };
		
		public static ILoader GetDefault()
		{ 
			#if UNITY_EDITOR
			if (_instance.TryGetInstance(l => l.GetType().Name == "AssetDatabaseLoader", out var i, _args))
				return i; 
			#else 
			if (_instance.TryGetInstance(l => l is RuntimeLoader, out var i, _args))
				return i; 
			#endif
			return null;
		}
	}
}
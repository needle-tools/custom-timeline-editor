using System;
using System.Collections.Generic;
using Needle.Timeline.Serialization;

namespace Needle.Timeline
{
	public interface ILoader
	{
		ISerializer Serializer { get; internal set; }
		bool Save(string id, ISerializationContext context, object @object);
		bool Load(string id, ISerializationContext context, out object obj);
		bool Rename(string oldId, string newId);
	}

	public class LoadersRegistry : ImplementorsRegistry<ILoader>
	{
		private static readonly LoadersRegistry _instance = new LoadersRegistry();
		private static readonly IList<IArgument> _args = new List<IArgument>() { new Argument(null, new JsonSerializer()) };
		
		public static ILoader GetDefault()
		{ 
			if (_instance.TryGetInstance(l => l is AssetDatabaseProvider, out var i, _args))
				return i; 
			return null;
		}
	}
}
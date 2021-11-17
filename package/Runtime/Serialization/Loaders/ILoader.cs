using System;

namespace Needle.Timeline
{
	public interface ILoader
	{
		ISerializer Serializer { get; internal set; }
		bool Save(string id, ISerializationContext context, object @object);
		bool Load(string id, ISerializationContext context, out object obj);
		bool Rename(string oldId, string newId, ISerializationContext context);
		// TODO: add and handle delete
	}
}
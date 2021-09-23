using System;

namespace Needle.Timeline.Serialization
{
	public interface ISerializer
	{
		object Serialize(object obj);
		T Deserialize<T>(object value);
		object Deserialize(object value, Type type);
	}
}
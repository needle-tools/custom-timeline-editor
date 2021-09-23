using System;

namespace Needle.Timeline
{
	public interface ISerializer
	{
		object Serialize(object obj);
		object Deserialize(Type type, object value);
	}
}
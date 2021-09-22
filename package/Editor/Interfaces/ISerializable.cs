namespace Needle.Timeline
{
	public interface ISerializer
	{
		
	}
	
	public interface ISerializable
	{
		string Serialize(ISerializer serializer);
		void Deserialize(ISerializer serializer, string data);
	}
}
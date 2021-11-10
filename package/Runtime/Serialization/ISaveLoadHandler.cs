namespace Needle.Timeline
{
	public interface ISaveLoadHandler
	{
		void Save(object obj);
		object Load();
	}
}
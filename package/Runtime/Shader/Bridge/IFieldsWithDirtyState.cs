namespace Needle.Timeline
{
	public interface IFieldsWithDirtyState
	{
		bool IsDirty(string fieldName);
		void SetDirty(string fieldName, bool dirty = true);
	}
}
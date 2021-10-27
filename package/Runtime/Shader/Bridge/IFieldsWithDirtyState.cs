namespace Needle.Timeline
{
	/// <summary>
	/// implement to control when a shader field (list) is updated (e.g. when you have a list of items that should be set once and then manipulated by the shader instead of being set every frame)
	/// </summary>
	public interface IFieldsWithDirtyState
	{
		bool IsDirty(string fieldName);
		void SetDirty(string fieldName, bool dirty = true);
	}
}
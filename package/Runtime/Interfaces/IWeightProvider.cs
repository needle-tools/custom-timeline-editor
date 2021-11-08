namespace Needle.Timeline
{
	/// <summary>
	/// To implement on type with ToolInfo
	/// </summary>
	public interface IWeightProvider<in T>
	{
		float GetCustomWeight(T context);
	}
}
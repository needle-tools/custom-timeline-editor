namespace Needle.Timeline
{
	/// <summary>
	/// Implement to receive onion skin render callback
	/// </summary>
	public interface IOnionSkin
	{
		/// <summary>
		/// callback to render onion skin
		/// </summary>
		/// <param name="level">before or after</param>
		void RenderOnionSkin(int level);
	}
}
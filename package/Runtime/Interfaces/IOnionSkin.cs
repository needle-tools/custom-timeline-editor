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
		/// <param name="layer">before or after</param>
		void RenderOnionSkin(int layer);
	}
}
using UnityEngine;

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
		void RenderOnionSkin(IOnionData data);
	}

	public interface IOnionData
	{
		int Layer { get; }
		Color ColorOnion { get; }
		float WeightOnion { get; }
		Color GetColor(Color col);
	}
}
using UnityEditor.Experimental.GraphView;
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
	}

	public struct OnionData : IOnionData
	{
		public int Layer { get; set; }
		public Color ColorOnion { get; set; }
		public float WeightOnion { get; set; }

		public OnionData(int layer)
		{
			Layer = layer;
			ColorOnion = Color.gray;
			WeightOnion = 0f;
			if (layer == 0) return;
			WeightOnion = 1f;
			if (layer < 0)
				ColorOnion = new Color(1f, .5f, .5f, .5f);
			else
				ColorOnion = new Color(0.5f, 1f, .5f, .5f);
		}

		public static OnionData Default { get; } = new OnionData()
		{
			Layer = 0,
			ColorOnion = Color.gray,
			WeightOnion = 0
		};
	}
}
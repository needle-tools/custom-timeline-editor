using UnityEngine;

namespace Needle.Timeline
{
	public struct OnionData : IOnionData
	{
		public int Layer { get; set; }
		public Color ColorOnion { get; set; }
		public float WeightOnion { get; set; }

		public Color GetColor(Color col)
		{
			return Color.Lerp(col, ColorOnion, WeightOnion);
		}

		public void SetColor(Color color)
		{
#if UNITY_EDITOR
			Gizmos.color = GetColor(color);
#endif
		}

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
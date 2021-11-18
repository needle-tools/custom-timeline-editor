using System.Collections;
using System.Collections.Generic;
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
		void SetColor(Color color);
	}

	public static class OnionSkinExtensions
	{
		public static void TryRender(this IOnionData data, IList list)
		{
			if (list == null) return;
			foreach (var e in list)
			{
				if (e == null) continue;
				if (e is IOnionSkin sk)
				{
					sk.RenderOnionSkin(data);
				}
				else return;
			}
		}
	}
}
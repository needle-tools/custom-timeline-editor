using System.Runtime.CompilerServices;
using UnityEngine;

namespace Needle.Timeline
{
	public static class DebugUtils
	{
		private static readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
		
		public static bool SetTexture(this Renderer renderer, Texture tex, string name = null)
		{
			if (!tex || !renderer) return false;
			var mat = renderer.sharedMaterial;
			if (!mat) return false;
			renderer.GetPropertyBlock(block);
			block.SetTexture(name ?? "_MainTex", tex); 
			renderer.SetPropertyBlock(block);
			return true;
		}
	}
}
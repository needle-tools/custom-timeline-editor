using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Needle.Timeline
{
	[AttributeUsage(AttributeTargets.Field)]
	public class TextureInfo : Attribute
	{
		public readonly int Width;
		public readonly int Height;
		public GraphicsFormat GraphicsFormat;
		public TextureFormat TextureFormat;
		public int? Depth;

		public TextureInfo(int width, int height)
		{
			Width = width;
			Height = height;
		}
	}
}
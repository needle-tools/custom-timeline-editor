#nullable enable

using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Needle.Timeline
{
	[AttributeUsage(AttributeTargets.Field)]
	public class Manual : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class TransformInfo : Attribute
	{
		// TODO: implement
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class TextureInfo : Attribute
	{
		public readonly int Width;
		public readonly int Height;
		public GraphicsFormat GraphicsFormat;
		public TextureFormat TextureFormat;
		public int Depth = 0;
		public FilterMode FilterMode = FilterMode.Bilinear;

		public TextureInfo(int width, int height)
		{
			Width = width;
			Height = height;
		}
	}


	[AttributeUsage(AttributeTargets.Field)]
	public abstract class BufferInfo : Attribute
	{
		public int Size;
		public int Stride;
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class ComputeBufferInfo : BufferInfo
	{
		public ComputeBufferType Type;
		public ComputeBufferMode Mode;

		public ComputeBufferInfo(){}

		public ComputeBufferInfo(int size, int stride)
		{
			this.Size = size;
			this.Stride = stride;
		}
	}
}
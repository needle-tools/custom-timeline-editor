#nullable enable

using System;
using Needle.Timeline.ResourceProviders;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Needle.Timeline
{
	[AttributeUsage(AttributeTargets.Field)]
	public class Manual : Attribute
	{
	}

	/// <summary>
	/// Use to set a field only ONCE to a shader. If you need dynamic control implement IFieldsWithDirtyState and set fields dirty individually
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class Once : Attribute
	{
		
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class TransformInfo : Attribute
	{
		// TODO: implement, should provide info of what we want of position, rotation, scale
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

		public RenderTextureDescription ToRenderTextureDescription()
		{
			var desc = new RenderTextureDescription
			{
				Width = Width,
				Height = Height,
				Depth = Depth,
				GraphicsFormat = GraphicsFormat,
				FilterMode = FilterMode
			};
			return desc;
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
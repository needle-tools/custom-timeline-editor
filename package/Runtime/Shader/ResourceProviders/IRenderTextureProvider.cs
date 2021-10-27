using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Needle.Timeline.ResourceProviders
{
	public interface ITextureDescription : IUnityObjectDescription
	{
		int Width { get; set; }
		int Height { get; set; }
		TextureFormat Format { get; set; }
		FilterMode FilterMode { get; set; }
		bool UseMipMap { get; set; }
		int MipCount { get; set; }
		GraphicsFormat? GraphicsFormat { get; set; }
	}

	public interface IRenderTextureDescription : ITextureDescription, IEquatable<RenderTexture>
	{
		bool RandomAccess { get; set; }
		int Depth { get; set; }
		new RenderTextureFormat Format { get; set; }
	}

	public struct RenderTextureDescription : IRenderTextureDescription
	{
		private TextureFormat format;
		
		public string Name { get; set; }
		public HideFlags HideFlags { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		TextureFormat ITextureDescription.Format
		{
			get => format;
			set => format = value;
		}
		public FilterMode FilterMode { get; set; }
		public bool UseMipMap { get; set; }
		public int MipCount { get; set; }
		public GraphicsFormat? GraphicsFormat { get; set; }
		public bool RandomAccess { get; set; }
		public int Depth { get; set; }
		public RenderTextureFormat Format { get; set; }

		public bool Equals(RenderTexture other)
		{
			if (!other) return false;
			return Width == other.width && 
			       Height == other.height && 
			       FilterMode == other.filterMode && 
			       UseMipMap == other.useMipMap && 
			       (GraphicsFormat == other.graphicsFormat || Format == other.format) && 
			       RandomAccess == other.enableRandomWrite && 
			       Depth == other.depth;
		}
	}

	public interface IRenderTextureProvider : IDisposable
	{
		RenderTexture GetTexture(string id, IRenderTextureDescription desc);
	}

	public class DefaultRenderTextureProvider : IRenderTextureProvider
	{
		private readonly Dictionary<string, RenderTexture> cache = new Dictionary<string, RenderTexture>();

		public void Dispose()
		{
			foreach (var c in cache.Values)
			{
				if (c && c.IsCreated()) c.Release();
			}
			cache.Clear();
		}

		public RenderTexture GetTexture(string id, IRenderTextureDescription desc)
		{
			if (desc.GraphicsFormat == null || desc.GraphicsFormat == GraphicsFormat.None)
			{
				desc.GraphicsFormat = GraphicsFormat.R8G8B8A8_SRGB;
			}
			if (cache.TryGetValue(id, out var rt))
			{
				rt = rt.SafeCreate(ref rt, desc);
				cache[id] = rt;
			}
			else
			{
				rt = ComputeBufferUtils.SafeCreate(null, ref rt, desc);
				cache.Add(id, rt);
			}
			return rt;
		}
	}
}
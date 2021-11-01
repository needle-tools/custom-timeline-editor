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
		TextureFormat? Format { get; set; }
		FilterMode FilterMode { get; set; }
		bool UseMipMap { get; set; }
		int MipCount { get; set; }
		GraphicsFormat? GraphicsFormat { get; set; }
		bool Validate();
	}

	public interface IRenderTextureDescription : ITextureDescription, IEquatable<RenderTexture>
	{
		bool RandomAccess { get; set; }
		int Depth { get; set; }
		new RenderTextureFormat? Format { get; set; }
	}
	
	public struct RenderTextureDescription : IRenderTextureDescription
	{
		private RenderTextureFormat? format;
		public string Name { get; set; }
		public HideFlags HideFlags { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		TextureFormat? ITextureDescription.Format { get; set; }

		public FilterMode FilterMode { get; set; }
		public bool UseMipMap { get; set; }
		public int MipCount { get; set; }
		public GraphicsFormat? GraphicsFormat { get; set; }

		public bool RandomAccess { get; set; }
		public int Depth { get; set; }

		public RenderTextureFormat? Format
		{
			get => format ?? RenderTextureFormat.Default;
			set => format = value;
		}

		public bool Validate()
		{
			// TODO: implement
			return Width > 0 && Height > 0;
		}

		public bool Equals(RenderTexture other)
		{
			if (!other) return false;
			var res = Width == other.width && 
			       Height == other.height && 
			       FilterMode == other.filterMode && 
			       UseMipMap == other.useMipMap && 
			       RandomAccess == other.enableRandomWrite && 
			       Depth == other.depth;
			if (!res) return false;
			
			if (Format != null)
			{
				if (Format == RenderTextureFormat.Default)
				{
					res = other.graphicsFormat == GraphicsFormatUtility.GetGraphicsFormat(Format.Value, RenderTextureReadWrite.Default);
				}
				else res = other.format == Format.Value;
			}
			if (!res) return false;
			
			return true;
		}
	}

	public interface IRenderTextureProvider : IDisposable
	{
		void ClearCaches();
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

		public void ClearCaches()
		{
			cache.Clear();
		}

		public RenderTexture GetTexture(string id, IRenderTextureDescription desc)
		{
			// if (desc.GraphicsFormat == null || desc.GraphicsFormat == GraphicsFormat.None)
			// {
			// 	desc.GraphicsFormat = GraphicsFormat.R8G8B8A8_SRGB;
			// }
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
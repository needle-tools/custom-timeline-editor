using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Needle.Timeline.ResourceProviders
{
	public interface ITextureDescription : IUnityObjectDescription
	{
		int Width { get; }
		int Height { get; }
		TextureFormat Format { get; }
		FilterMode FilterMode { get; }
		bool MipChain { get; }
		int MipCount { get; }
	}

	public interface IRenderTextureDescription : ITextureDescription
	{
		public bool RandomWrite { get; }
	}

	public interface IRenderTextureProvider : IDisposable
	{
		RenderTexture GetTexture(string id,
			int width,
			int height,
			int depth,
			GraphicsFormat? graphicsFormat = null,
			bool? randomWrite = null,
			Action<RenderTexture> beforeCreate = null);
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

		public RenderTexture GetTexture(string id,
			int width,
			int height,
			int depth,
			GraphicsFormat? graphicsFormat = null,
			bool? randomWrite = null,
			Action<RenderTexture> beforeCreate = null)
		{
			if (graphicsFormat == null || graphicsFormat == GraphicsFormat.None)
			{
				graphicsFormat = GraphicsFormat.R8G8B8A8_SRGB;
			}
			if (cache.TryGetValue(id, out var rt))
			{
				rt = rt.SafeCreate(ref rt, width, height, depth, (GraphicsFormat)graphicsFormat, randomWrite ?? false, beforeCreate);
				cache[id] = rt;
			}
			else
			{
				rt = ComputeBufferUtils.SafeCreate(null, ref rt, width, height, depth, (GraphicsFormat)graphicsFormat, randomWrite ?? false, beforeCreate);
				cache.Add(id, rt);
			}
			return rt;
		}
	}
}
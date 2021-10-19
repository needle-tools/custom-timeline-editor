using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Needle.Timeline
{
	public interface IComputeBufferProvider : IDisposable
	{
		ComputeBuffer GetBuffer(string id, int count, int stride, ComputeBufferType? type = null, ComputeBufferMode? mode = null);
		void DisposeBuffer(string id);
	}

	public interface IRenderTextureProvider : IDisposable
	{
		RenderTexture GetTexture(string id, int width, int height, int depth, 
			GraphicsFormat? graphicsFormat = null, bool? randomWrite = null);
	}

	public interface IResourceProvider
	{
		IComputeBufferProvider ComputeBufferProvider { get; }
		IRenderTextureProvider RenderTextureProvider { get; }
	}

	public static class DefaultResources
	{
		private static IComputeBufferProvider _cb;
		public static IComputeBufferProvider GlobalComputeBufferProvider => _cb ??= new DefaultComputeBufferProvider();
	}

	public class ResourceProvider : IResourceProvider
	{
		public static IResourceProvider CreateDefault() => new ResourceProvider(
			new DefaultComputeBufferProvider(), 
			new DefaultRenderTextureProvider()
			);
		
		public ResourceProvider(IComputeBufferProvider computeBufferProvider, IRenderTextureProvider renderTextureProvider)
		{
			ComputeBufferProvider = computeBufferProvider;
			RenderTextureProvider = renderTextureProvider;
		}

		public IComputeBufferProvider ComputeBufferProvider { get; }
		public IRenderTextureProvider RenderTextureProvider { get; }
	}

	public class DefaultRenderTextureProvider : IRenderTextureProvider
	{
		private readonly Dictionary<string, RenderTexture> cache = new Dictionary<string, RenderTexture>();
		
		public void Dispose()
		{
			foreach (var c in cache.Values)
			{
				if(c && c.IsCreated()) c.Release();
			}
			cache.Clear(); 
		}

		public RenderTexture GetTexture(string id, int width, int height, int depth, GraphicsFormat? graphicsFormat = null, bool? randomWrite = null)
		{
			if(cache.TryGetValue(id, out var rt))
			{
				rt = rt.SafeCreate(ref rt, width, height, depth, graphicsFormat ?? GraphicsFormat.None, randomWrite ?? false);
				cache[id] = rt;
			}
			else
			{
				rt = ComputeBufferUtils.SafeCreate(null, ref rt, width, height, depth, graphicsFormat ?? GraphicsFormat.None, randomWrite ?? false);
				cache.Add(id, rt);
			}
			return rt;
		}
	}

	public class DefaultComputeBufferProvider : IComputeBufferProvider
	{
		private readonly Dictionary<string, ComputeBuffer> cache = new Dictionary<string, ComputeBuffer>();

		public ComputeBuffer GetBuffer(string id, int count, int stride, ComputeBufferType? type = null, ComputeBufferMode? mode = null)
		{
			if (cache.TryGetValue(id, out var buffer))
			{
				buffer = ComputeBufferUtils.SafeCreate(ref buffer, count, stride, type, mode);
				cache[id] = buffer;
			}
			else
			{
				buffer = ComputeBufferUtils.SafeCreate(ref buffer, count, stride);
				cache.Add(id, buffer);
			}
			
			return buffer;
		}

		public void DisposeBuffer(string id)
		{
			if (cache.TryGetValue(id, out var buffer) && buffer.IsValid())
			{
				buffer.Dispose();
			}
		}

		public void Dispose()
		{
			foreach (var buf in cache.Values)
			{
				if(buf.IsValid())
					buf.Dispose();
			}
			cache.Clear();
		}
	}
}
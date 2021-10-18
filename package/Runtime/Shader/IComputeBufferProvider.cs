using System;
using System.Collections.Generic;
using UnityEngine;

namespace Needle.Timeline
{
	public interface IComputeBufferProvider : IDisposable
	{
		ComputeBuffer GetBuffer(string id, int count, int stride, ComputeBufferType? type = null, ComputeBufferMode? mode = null);
		void DisposeBuffer(string id);
	}

	public interface ITextureProvider : IDisposable
	{
		Texture GetTexture(string id, int width, int height);
	}

	public interface IResourceProvider
	{
		IComputeBufferProvider ComputeBufferProvider { get; }
	}

	public static class DefaultResources
	{
		private static IComputeBufferProvider _cb;
		public static IComputeBufferProvider GlobalComputeBufferProvider => _cb ??= new DefaultComputeBufferProvider();
	}

	public class ResourceProvider : IResourceProvider
	{
		public ResourceProvider(IComputeBufferProvider computeBufferProvider)
		{
			ComputeBufferProvider = computeBufferProvider;
		}

		public IComputeBufferProvider ComputeBufferProvider { get; }
	}

	public class DefaultComputeBufferProvider : IComputeBufferProvider
	{
		private readonly Dictionary<string, ComputeBuffer> _buffers = new Dictionary<string, ComputeBuffer>();

		public ComputeBuffer GetBuffer(string id, int count, int stride, ComputeBufferType? type = null, ComputeBufferMode? mode = null)
		{
			if (_buffers.TryGetValue(id, out var buffer))
			{
				buffer = ComputeBufferUtils.SafeCreate(ref buffer, count, stride, type, mode);
				_buffers[id] = buffer;
			}
			else
			{
				buffer = ComputeBufferUtils.SafeCreate(ref buffer, count, stride);
				_buffers.Add(id, buffer);
			}
			
			return buffer;
		}

		public void DisposeBuffer(string id)
		{
			if (_buffers.TryGetValue(id, out var buffer) && buffer.IsValid())
			{
				buffer.Dispose();
			}
		}

		public void Dispose()
		{
			foreach (var buf in _buffers.Values)
			{
				if(buf.IsValid())
					buf.Dispose();
			}
		}
	}
}
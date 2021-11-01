using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Needle.Timeline.ResourceProviders
{
	// TODO: remove before create callback and replace with descriptor parameter

	
	public interface IComputeBufferProvider : IDisposable
	{
		void ClearCaches();
		ComputeBuffer GetBuffer(string id, IComputeBufferDescription desc);
		void DisposeBuffer(string id);
	}

	public class DefaultComputeBufferProvider : IComputeBufferProvider
	{
		private readonly Dictionary<string, ComputeBuffer> cache = new Dictionary<string, ComputeBuffer>();
		public bool MaxCount = true;

		public void ClearCaches()
		{
			cache.Clear();
		}

		public ComputeBuffer GetBuffer(string id, IComputeBufferDescription desc)
		{
			if (cache.TryGetValue(id, out var buffer))
			{
				var bufferCount = buffer?.IsValid() ?? false ? buffer.count : -1; 
				desc.Size = MaxCount ? Mathf.Max(desc.Size, bufferCount) : desc.Size;
				buffer = ComputeBufferUtils.SafeCreate(ref buffer, desc);
				cache[id] = buffer;
			}
			else
			{
				buffer = ComputeBufferUtils.SafeCreate(ref buffer, desc);
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
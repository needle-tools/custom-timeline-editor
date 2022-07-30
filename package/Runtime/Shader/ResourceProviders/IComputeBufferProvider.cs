using System;
using System.Collections.Generic;
using UnityEditorInternal;
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

	public enum GrowStrategy
	{
		NoGrowing = 0,
		Exact = 1,
		Double = 2
	}

	public class DefaultComputeBufferProvider : IComputeBufferProvider
	{
		private readonly Dictionary<string, ComputeBuffer> cache = new Dictionary<string, ComputeBuffer>();

		public GrowStrategy GrowStrategy { get; set; } = GrowStrategy.Double;

		public void ClearCaches()
		{
			cache.Clear();
		}

		public ComputeBuffer GetBuffer(string id, IComputeBufferDescription desc)
		{
			if (cache.TryGetValue(id, out var buffer))
			{
				var bufferCount = buffer?.IsValid() ?? false ? buffer.count : -1; 
				if(bufferCount < desc.Size)
				{
					switch (GrowStrategy)
					{
						case GrowStrategy.Exact:
							desc.Size = Mathf.Max(desc.Size, bufferCount);
							break;
						case GrowStrategy.Double:
							while(bufferCount < desc.Size) bufferCount *= 2;
							desc.Size = bufferCount;
							break;
					}
				}
				// dont shrink the buffer
				if (GrowStrategy != GrowStrategy.NoGrowing)
					desc.Size = bufferCount;
				
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
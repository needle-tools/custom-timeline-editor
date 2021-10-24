using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Needle.Timeline
{
	public static class ComputeBufferProvider
	{
		private static readonly Dictionary<string, ComputeBuffer> _buffers = new Dictionary<string, ComputeBuffer>();

		public static ComputeBuffer GetBuffer<T>(string id, List<T> data, int stride, int? size = null) where T : struct
		{
			if (_buffers.TryGetValue(id, out var buffer))
			{
				buffer = ComputeBufferUtils.SafeCreate(ref buffer, size ?? data.Count, stride);
				_buffers[id] = buffer;
			}
			else
			{
				buffer = ComputeBufferUtils.SafeCreate(ref buffer, size ?? data.Count, stride);
				_buffers.Add(id, buffer);
			}
			
			buffer.SetData(data); 
			return buffer;
		}
	}
	
	public static class ComputeBufferUtils
	{
		public static RenderTexture SafeCreate(this RenderTexture _, ref RenderTexture tex, int width, int height, int depth, 
			GraphicsFormat format, bool randomWriteEnabled = false, Action<RenderTexture> beforeCreate = null)
		{
			if (!tex || tex.width != width || tex.height != height || tex.depth != depth || tex.graphicsFormat != format || tex.enableRandomWrite != randomWriteEnabled)
			{
				if(tex && tex.IsCreated()) tex.Release();
				tex = new RenderTexture(width, height, depth, format);
				tex.enableRandomWrite = randomWriteEnabled;
				beforeCreate?.Invoke(tex);
				tex.Create();
				Debug.Log("Create RT");
			}
			else if (tex && !tex.IsCreated()) tex.Create();
			return tex;
		}

		public static ComputeBuffer SafeCreate(ref ComputeBuffer buffer, int size, int stride, ComputeBufferType? type = null, ComputeBufferMode? mode = null)
		{
			if (buffer == null || !buffer.IsValid() || buffer.count != size || buffer.stride != stride)
			{
				buffer.SafeDispose();
				buffer = new ComputeBuffer(size, stride, type ?? ComputeBufferType.Default, mode ?? ComputeBufferMode.Immutable);
				Debug.Log("Create ComputeBuffer, size=" + size + ", stride=" + stride);
			}
			return buffer;
		}
		
		// https://forum.unity.com/threads/graphicsbuffer-mesh-vertices-and-compute-shaders.777548/#post-6989081
		// https://github.com/cecarlsen/HowToDrawATriangle
		public static GraphicsBuffer SafeCreate(ref GraphicsBuffer buffer, GraphicsBuffer.Target target, int size, int stride)
		{
			if (buffer == null || !buffer.IsValid() || buffer.count < size || buffer.stride != stride)
			{
				buffer.SafeDispose();
				buffer = new GraphicsBuffer(target, size, stride);
			}
			return buffer;
		}
	}
}
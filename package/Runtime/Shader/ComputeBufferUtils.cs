using System;
using System.Collections.Generic;
using Needle.Timeline.ResourceProviders;
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
				tex.hideFlags = HideFlags.DontSaveInEditor;
				tex.enableRandomWrite = randomWriteEnabled;
				beforeCreate?.Invoke(tex);
				tex.Create();
				Debug.Log("Create RT");
			}
			else if (tex && !tex.IsCreated()) tex.Create();
			return tex;
		}
		
		public static RenderTexture SafeCreate(this RenderTexture _, ref RenderTexture tex, IRenderTextureDescription desc)
		{
			if (!tex || !desc.Equals(tex))
			{
				if(tex && tex.IsCreated()) tex.Release();
				tex = new RenderTexture(desc.Width, desc.Height, desc.Depth, desc.Format);
				tex.hideFlags = HideFlags.DontSaveInEditor;
				tex.enableRandomWrite = desc.RandomAccess;
				tex.Create();
				Debug.Log("Create RT");
			}
			else if (tex && !tex.IsCreated()) tex.Create();
			return tex;
		}

		public static ComputeBuffer SafeCreate(ref ComputeBuffer buffer, int size, int stride, ComputeBufferType? type = null, ComputeBufferMode? mode = null)
		{
			return SafeCreate(ref buffer,
				new ComputeBufferDescription() { Size = size, Stride = stride, Type = type.GetValueOrDefault(), Mode = mode.GetValueOrDefault() });
		}

		public static ComputeBuffer SafeCreate(ref ComputeBuffer buffer, IComputeBufferDescription desc)
		{
			if (buffer == null || !buffer.IsValid() || buffer.count != desc.Size || buffer.stride != desc.Stride)
			{
				buffer.SafeDispose();
				buffer = new ComputeBuffer(desc.Size, desc.Stride, desc.Type, desc.Mode);
				buffer.name = desc.Name;
				Debug.Log("Create ComputeBuffer, size=" + desc.Size + ", stride=" + desc.Stride);
			}
			if(desc.CounterValue != null)
				buffer.SetCounterValue(desc.CounterValue.Value);
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
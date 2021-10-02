using System;
using UnityEngine;

namespace Needle.Timeline
{
	public static class ComputeBufferUtils
	{
		public static ComputeBuffer SafeCreate(ref ComputeBuffer buffer, int size, int stride)
		{
			if (buffer == null || !buffer.IsValid() || buffer.count < size || buffer.stride != stride)
			{
				buffer.SafeDispose();
				buffer = new ComputeBuffer(size, stride);
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
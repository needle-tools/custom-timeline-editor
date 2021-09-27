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
	}
}
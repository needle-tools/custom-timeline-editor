using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine;

namespace Needle.Timeline
{
	public static class ComputeShaderUtils
	{
		public static void SetTexture(this ComputeShader shader, string kernel, string name, Texture texture)
		{
			var index = shader.FindKernel(kernel);
			shader.SetTexture(index, name, texture);
		}

		public static void SetBuffer(this ComputeShader shader, string kernel, string name, ComputeBuffer buffer)
		{
			var index = shader.FindKernel(kernel);
			shader.SetBuffer(index, name, buffer);
		}

		public static ComputeBuffer SetBuffer<T>(this ComputeShader shader, string kernelName, string name, List<T> data, int stride, int? size = null) where T : struct
		{
			var kernel = shader.FindKernel(kernelName);
			return SetBuffer(shader, kernel, name, data, stride, size);
		}

		public static ComputeBuffer SetBuffer<T>(this ComputeShader shader, int kernel, string id, List<T> data, int stride, int? size = null) where T : struct
		{
			if (data.Count <= 0) return null;
			var buffer = ComputeBufferProvider.GetBuffer(id, data, stride, size);
			shader.SetBuffer(kernel, id, buffer);
			return buffer;
		}

		public static bool DispatchOptimal(this ComputeShader shader, string kernel, int x, int y, int z)
		{
			var index = shader.FindKernel(kernel);
			return shader.DispatchOptimal(index, x, y, z);
		}

		public static bool DispatchOptimal(this ComputeShader shader, int kernel, int x, int y, int z)
		{
			if (x <= 0 || y <= 0 || z <= 0) return false;
			shader.GetKernelThreadGroupSizes(kernel, out var tx, out var ty, out var tz);
			tx = (uint)Mathf.CeilToInt(x / (float)tx);
			ty = (uint)Mathf.CeilToInt(y / (float)ty);
			tz = (uint)Mathf.CeilToInt(z / (float)tz);
			if (tx <= 0 || ty <= 0 || tz <= 0) return false;
			shader.Dispatch(kernel, (int)tx, (int)ty, (int)tz);
			return true;
		}

		public static void SetTime(this ComputeShader shader)
		{
			shader.SetVector("_Time", new Vector4(Time.time, Mathf.Sin(Time.time), Mathf.Cos(Time.time), Time.deltaTime));
		}
	}
}
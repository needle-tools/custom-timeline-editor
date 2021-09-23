using System;
using System.CodeDom;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Needle.Timeline
{
	public class ComputeBufferInterpolator : IInterpolator<ComputeBuffer>
	{
		public ComputeShader Shader;
		public int Kernel;
		public ComputeBuffer Output;
		public int Tx = 32;

		public ComputeBufferInterpolator(ComputeShader shader, int kernel = 0)
		{
			Shader = shader;
			this.Kernel = kernel;
		}

		public ComputeBufferInterpolator(Type type, int kernel = 0)
		{
			if (type == typeof(float))
			{
				
			}
		}

		public bool CanInterpolate(Type type)
		{
			return false;
		}

		public object Interpolate(object v0, object v1, float t)
		{
			throw new NotImplementedException();
		}

		public ComputeBuffer Interpolate(ComputeBuffer v0, ComputeBuffer v1, float t)
		{
			var count = Mathf.Max(v0.count, v1.count);
			var stride = Mathf.Max(v0.stride, v1.stride);
			if (Output == null || !Output.IsValid() || count != Output.count || stride != Output.stride) 
			{
				Output = new ComputeBuffer(count, stride);
			}

			Shader.SetBuffer(Kernel, "Buffer0", v0);
			Shader.SetBuffer(Kernel, "Buffer1", v1);
			Shader.SetBuffer(Kernel, "Output", Output);
			var tx = Mathf.CeilToInt(count / (float)Tx);
			Shader.Dispatch(Kernel, tx, 1, 1);
			return Output;
		}
	}
}
#nullable enable
using System.Collections.Generic;
using UnityEngine;
using UnityEngineInternal;

namespace Needle.Timeline
{
	public static class ComputeShaderExtensions_Runner
	{
		private static readonly Dictionary<object, List<ComputeShaderRunner>> _runnersCache = new Dictionary<object, List<ComputeShaderRunner>>();

		public static void Dispose(this ComputeShader _, object owner)
		{
			if (_runnersCache.TryGetValue(owner, out var runners))
			{
				foreach (var run in runners)
				{
					run.Dispose();
				}
				runners.Clear();
			}
		}

		public static bool Run<T1, T2, T3>(this ComputeShader shader, object owner, string kernelName, T1 x, T2 y, T3 z)
		{
			if (!shader) return false;
			if (_runnersCache.TryGetValue(owner, out var runners))
			{
				for (var index = 0; index < runners.Count; index++)
				{
					var runner = runners[index];
					// if the shader got disposed
					if (!runner.Shader)
					{
						runner.Dispose();
						runners.RemoveAt(index);
						index -= 1;
						continue;
					}
					if (runner.Shader == shader)
					{
						return runner.Run(kernelName, x, y, z);
					}
				}
			}
			else
			{
				var list = new List<ComputeShaderRunner>();
				_runnersCache.Add(owner, list);
				var runner = new ComputeShaderRunner(owner, shader);
				list.Add(runner);
				return runner.Run(kernelName, x, y, z);
			}


			return false;
		}
	}
}
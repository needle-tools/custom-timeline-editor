using UnityEditor.Android;
using UnityEngine;

namespace Needle.Timeline
{
	public static class ComputeShaderUtils
	{
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
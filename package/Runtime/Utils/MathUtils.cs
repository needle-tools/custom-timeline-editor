using UnityEngine;

namespace Needle.Timeline
{
	public static class MathUtils
	{
		public static float Remap(this float value, float from1, float to1, float from2, float to2)
		{
			return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
		}

		public static bool ApproximatelyZeroLength(this Vector3 vec)
		{
			if (vec == Vector3.zero) return true;
			const float max = 0.001f;
			var zx = Mathf.Abs(vec.x) <= max;
			var zy = Mathf.Abs(vec.y) <= max;
			var zz = Mathf.Abs(vec.z) <= max;
			return zx && zy && zz;
		}
	}
}
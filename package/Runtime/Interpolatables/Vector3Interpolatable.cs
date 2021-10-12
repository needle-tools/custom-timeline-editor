using UnityEngine;

namespace Needle.Timeline
{
	public class ColorInterpolatable : Interpolatable<Color>
	{
		public override void Interpolate(ref Color instance, Color t0, Color t1, float t)
		{
			instance = Color.Lerp(t0, t1, t);
		}
	}
	
	public class Vector4Interpolatable : Interpolatable<Vector4>
	{
		public override void Interpolate(ref Vector4 instance, Vector4 t0, Vector4 t1, float t)
		{
			instance = Vector4.Lerp(t0, t1, t);
		}
	}
	
	public class Vector3Interpolatable : Interpolatable<Vector3>
	{
		public override void Interpolate(ref Vector3 instance, Vector3 t0, Vector3 t1, float t)
		{
			instance = Vector3.Lerp(t0, t1, t);
		}
	}
	
	public class Vector2Interpolatable : Interpolatable<Vector2>
	{
		public override void Interpolate(ref Vector2 instance, Vector2 t0, Vector2 t1, float t)
		{
			instance = Vector2.Lerp(t0, t1, t);
		}
	}
}
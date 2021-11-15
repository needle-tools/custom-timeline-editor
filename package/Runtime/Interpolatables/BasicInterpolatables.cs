using System;
using System.CodeDom;
using UnityEngine;
// ReSharper disable UnusedType.Global

namespace Needle.Timeline
{
	
	[Priority(-1000)]
	public class FallbackInterpolatable : IInterpolatable<object>
	{
		public void Interpolate(ref object instance, object t0, object t1, float t)
		{
			instance = t < 1 ? t0 : t1;
		}
	}
	
	[Interpolatable(typeof(Enum))]
	public class EnumInterpolatable : Interpolatable<Enum>, IInterpolatable
	{
		public override void Interpolate(ref Enum instance, Enum t0, Enum t1, float t)
		{
			instance = t > .5f ? t1 : t0;
		}
	}
	
	public class IntInterpolatable : Interpolatable<int>
	{
		public override void Interpolate(ref int instance, int t0, int t1, float t)
		{
			instance = Mathf.RoundToInt(Mathf.LerpUnclamped(t0, t1, t));
		}
	}
	
	public class FloatInterpolatable : Interpolatable<float>
	{
		public override void Interpolate(ref float instance, float t0, float t1, float t)
		{
			instance = Mathf.LerpUnclamped(t0, t1, t);
		}
	}
	
	public class QuaternionInterpolatable : Interpolatable<Quaternion>
	{
		public override void Interpolate(ref Quaternion instance, Quaternion t0, Quaternion t1, float t)
		{
			instance = Quaternion.Lerp(t0, t1, t);
		}
	}
	
	public class ColorInterpolatable : Interpolatable<Color>
	{
		public override void Interpolate(ref Color instance, Color t0, Color t1, float t)
		{
			// if (t < 0)
			// {
			// 	// Color.LerpUnclamped()
			// }
			instance = Color.LerpUnclamped(t0, t1, t);
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
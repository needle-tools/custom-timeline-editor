using Needle.Timeline;
using UnityEngine;


public struct Line : IInterpolatable<Line>
{
	public Vector3 Start;
	public Vector3 End;
	

	public void Interpolate(ref Line instance, Line t0, Line t1, float t)
	{
		instance.Start = Vector3.Lerp(t0.Start, t1.Start, t);
		instance.End = Vector3.Lerp(t0.End, t1.End, t);
	}

	public void Interpolate(ref object instance, object t0, object t1, float t)
	{
		this.Cast(ref instance, t0, t1, t);
	}
}
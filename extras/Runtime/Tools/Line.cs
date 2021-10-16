using Needle.Timeline;
using UnityEditor;
using UnityEngine;


public struct Line : IModifySelf, IInit
{
	public Vector3 Start;
	public Vector3 End;

	public bool OnInput(IToolData data)
	{
#if UNITY_EDITOR
		if (data.WorldPosition != null)
		{
			var sp = data.ToScreenPoint(data.WorldPosition.Value);
			var dist = 50;
			if (Vector2.Distance(sp, data.ToScreenPoint(Start)) > dist
			    && Vector2.Distance(sp, data.ToScreenPoint(End)) > dist)
				return false;
		}
		var start = Handles.PositionHandle(Start, Quaternion.identity);
		var end = Handles.PositionHandle(End, Quaternion.identity);
		var changed = start != Start || end != End;
		Start = start;
		End = end;
		return changed;
#else
		return false;
#endif
	}

	public void Init(InitStage stage, IToolData data)
	{
		if (stage == InitStage.BasicValuesSet && data != null)
			End = Start + data.DeltaWorld.GetValueOrDefault().normalized;
	}

	public void DrawGizmos()
	{
		Gizmos.DrawLine(Start, End);
		var dir = End - Start;
		var ort = Vector3.Cross(dir * .1f, Vector3.forward);
		Gizmos.DrawLine(End, Vector3.Lerp(Start, End + ort, .9f));
		ort *= -1;
		Gizmos.DrawLine(End, Vector3.Lerp(Start, End + ort, .9f));
	}
}
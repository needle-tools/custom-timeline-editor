using Needle.Timeline;
using UnityEditor;
using UnityEngine;


public struct Line : IModifySelf, IInit
{
	public Vector3 Start;
	public Vector3 End;

	public bool OnInput()
	{
#if UNITY_EDITOR
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
			End = Start + data.DeltaWorld.GetValueOrDefault();
	}

	public void DrawGizmos()
	{
		Gizmos.DrawLine(Start, End);
		Gizmos.DrawSphere(Start, .08f);
	}
}
using System;
using Needle.Timeline;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;


public struct Direction : ICustomControls, IToolEvents
{
	public Vector3 Start;
	public Vector3 End;

	// the following is not an option because types should easily be set to shaders
	// [NonSerialized, JsonIgnore]
	// private Vector3 _deltaSum;

	public void OnToolEvent(ToolStage stage, IToolData data)
	{
		if (data == null) return;
		if (stage == ToolStage.BasicValuesSet)
		{
			// Debug.Log(data.DeltaWorld.GetValueOrDefault());
			End = Start + data.DeltaWorld.GetValueOrDefault().normalized;
		}
		else if (stage == ToolStage.InputUpdated)
		{
			// if (stage == CreationStage.BasicValuesSet) _deltaSum = data.DeltaWorld.GetValueOrDefault().normalized;
			End += data.DeltaWorld.GetValueOrDefault() * 0.005f;
		}
	}

	public bool OnCustomControls(IToolData data, IToolModule module)
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
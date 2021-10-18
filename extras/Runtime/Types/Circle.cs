using System;
using Needle.Timeline;
using UnityEditor;
using UnityEngine;


public struct Circle : IToolEvents, ICustomControls, IOnionSkin
{
	public Vector3 Position;
	public float Radius;

	public void OnToolEvent(ToolStage stage, IToolData? data)
	{
		if (data == null) return;
		if (data.WorldPosition != null && data.StartWorldPosition != null)
			Radius = (data.WorldPosition.Value - data.StartWorldPosition.Value).magnitude * .1f;
	}

	public bool OnCustomControls(IToolData data, IToolModule tool)
	{
#if UNITY_EDITOR
		var sp = data.ToScreenPoint(Position);
		var dist = (sp - data.ScreenPosition).magnitude;
		// Handles.Label(Position, sp + "\n" + dist + "\n" + data.ScreenPosition + "\n" + Screen.height);
		if (dist > Screen.width * .25f) return false;
		Handles.color = new Color(0, 1, 1, .5f);
		var newRadius = Handles.RadiusHandle(Quaternion.LookRotation(Camera.current.transform.forward), Position, Radius, true);
		var changed = Math.Abs(newRadius - Radius) > Mathf.Epsilon;
		Radius = newRadius;
		return changed;
#else
				return false;
#endif
	}

	public void RenderOnionSkin(IOnionData data)
	{
#if UNITY_EDITOR
		Handles.color = Color.gray;
		Handles.color = Color.Lerp(Handles.color, data.ColorOnion, data.WeightOnion);
		if (Radius > 2)
		{
			Gizmos.color = Handles.color;
			Gizmos.DrawSphere(Position, .05f);
		}
		Handles.DrawWireDisc(Position, Camera.current.transform.forward, Radius);
#endif
	}
}
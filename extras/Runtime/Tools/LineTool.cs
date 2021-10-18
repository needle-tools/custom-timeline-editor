#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Needle.Timeline;
using UnityEditor;
using UnityEngine;
using Handles = UnityEditor.Handles;

public class LineTool : CustomClipToolBase
{
	private Vector3? startPt = null;

	protected override bool OnSupports(Type type)
	{
		return typeof(List<Direction>).IsAssignableFrom(type);
	}

	protected override void OnHandleInput()
	{
		if (Event.current.modifiers != EventModifiers.None) return;
		if (Event.current.button != 0) return;
		if (Event.current.type == EventType.MouseDown)
		{
			if (startPt == null)
			{
				startPt = GetCurrentMousePositionInScene();
				UseEvent();
			}
		}
		else if (Event.current.type == EventType.MouseDrag)
		{
			if (startPt != null)
			{
				var endpt = GetCurrentMousePositionInScene();
				Handles.DrawLine(startPt.Value, endpt);
				// Debug.DrawLine(startPt.Value, GetCurrentMousePositionInScene(), Color.white, 0);
				UseEvent();

				// CreateLineMaterial();
				// lineMaterial.SetPass(0);
				// GL.PushMatrix();
				// GL.Begin(GL.LINES);
				// GL.Color(Color.red);
				// GL.Vertex(startPt.Value);
				// GL.Vertex(endpt);
				// GL.End();
				// GL.PopMatrix();
			}
		}
		else if (Event.current.type == EventType.MouseUp)
		{
			if (startPt != null)
			{
				UseEvent();
				var target = Targets.LastOrDefault();
				if (target.IsNull()) return;
				var clip = target.Clip;
				var time = (float)target.Time;
				var kf = clip.GetClosest(time);
				var guide = new Direction() { Start = startPt.GetValueOrDefault(), End = GetCurrentMousePositionInScene() };
				if (kf != null && Mathf.Abs(time - kf.time) < .1f)
				{
					var lines = kf.value as List<Direction>;
					lines.Add(guide);
					kf.RaiseValueChangedEvent();
				}
				else
				{
					clip.Add(new CustomKeyframe<List<Direction>>(new List<Direction>() { guide }, time));
				}
				startPt = null;
			}
		}
	}
}
#endif
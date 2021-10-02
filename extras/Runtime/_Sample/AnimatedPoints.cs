using System;
using System.Collections.Generic;
using System.Linq;
using Needle.Timeline;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace _Sample._Sample
{
	public struct Guide
	{
		public Vector3 Start;
		public Vector3 End;
	}

#if UNITY_EDITOR
	public class GuideTool : CustomClipToolBase
	{
		private Vector3? startPt = null;

		protected override bool OnSupports(Type type)
		{
			return typeof(List<Guide>).IsAssignableFrom(type);
		}

		protected override void OnInput(EditorWindow window)
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
					var guide = new Guide() { Start = startPt.GetValueOrDefault(), End = GetCurrentMousePositionInScene() };
					if (kf != null && Mathf.Abs(time - kf.time) < .1f)
					{
						var guides = kf.value as List<Guide>;
						guides.Add(guide);
						kf.RaiseValueChangedEvent();
					}
					else
					{
						clip.Add(new CustomKeyframe<List<Guide>>(new List<Guide>() { guide }, time));
					}
					startPt = null;
				}
			}
		}
	}
#endif

	[ExecuteAlways]
	public class AnimatedPoints : MonoBehaviour, IAnimated
	{
		[Animate(Interpolator = typeof(NoInterpolator)), NonSerialized]
		public List<Vector3> points = new List<Vector3>();

		[Animate, NonSerialized] public List<Guide> guides = new List<Guide>();

		[Animate] public float gizmoSizeFactor = 1;

		[Animate] public float Factor = 1;

		private Guide Interpolate(Guide g0, Guide g1, float t)
		{
			return new Guide();
		}

		public int pointsCount => points?.Count ?? 0;

		private Color[] colors = new[]
			{ new Color(0.9f, .5f, 0), new Color(0.5f, 0.9f, .5f), new Color(.9f, .8f, .2f), new Color(0.2f, .9f, .8f), new Color(.7f, .2f, .9f) };

		private void OnDrawGizmos()
		{
			var size = Vector3.up * .01f;
			if (points != null)
			{
				for (var index = 0; index < points.Count; index++)
				{
					Gizmos.color = colors[index % colors.Length];
					var pt = points[index];
					Gizmos.DrawLine(pt, pt + size);
					Gizmos.DrawSphere(pt, .1f * gizmoSizeFactor);
				}
			}

			if (guides != null)
			{
				Gizmos.color = Color.cyan;
				foreach (var guide in guides)
				{
					Debug.DrawLine(guide.Start, guide.End);
				}
			}
		}
	}
}
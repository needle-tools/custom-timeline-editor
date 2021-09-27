using System;
using System.Collections.Generic;
using Needle.Timeline;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

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
		private Vector3? lastPt = null;
		
		public override bool Supports(Type type)
		{
			return typeof(List<Guide>).IsAssignableFrom(type);
		}

		protected override void OnToolInput()
		{
			if (Event.current.type == EventType.MouseDown && Event.current.modifiers == EventModifiers.None)
			{
				if (lastPt != null)
				{
					var clip = ActiveClip;
					var kf = clip.GetClosest((float)CurrentTime);
					var guide = new Guide() { Start = lastPt.GetValueOrDefault(), End = GetCurrentMousePositionInScene() };
					if (kf != null && Mathf.Abs((float)CurrentTime - kf.time) < .1f)
					{
						var guides = kf.value as List<Guide>;
						guides.Add(guide);
						kf.RaiseValueChangedEvent();
					}
					else
					{
						clip.Add(new CustomKeyframe<List<Guide>>(new List<Guide>() { guide }, (float)CurrentTime));
					}
					lastPt = null;
				}
				else
				{
					lastPt = GetCurrentMousePositionInScene();
				}
			}
		}
	}
#endif

	[ExecuteAlways]
	public class AnimatedPoints : MonoBehaviour, IAnimated
	{
		[Animate, NonSerialized] public List<Vector3> points = new List<Vector3>();

		[Animate, NonSerialized] public List<Guide> guides = new List<Guide>();

		[Animate] public float gizmoSizeFactor = 1;

		public int pointsCount => points?.Count ?? 0;

		private void OnDrawGizmos()
		{
			var size = Vector3.up * .01f;
			if (points != null)
			{
				Gizmos.color = Color.yellow;
				for (var index = 0; index < points.Count; index++)
				{
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
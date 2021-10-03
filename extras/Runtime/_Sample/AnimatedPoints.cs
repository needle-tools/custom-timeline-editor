using System;
using System.Collections.Generic;
using Needle.Timeline;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace _Sample._Sample
{
	[ExecuteAlways]
	public class AnimatedPoints : MonoBehaviour, IAnimated
	{
		[Animate(AllowInterpolation = false), NonSerialized]
		public List<Vector3> points = new List<Vector3>();

		[Animate, NonSerialized] public List<Line> guides = new List<Line>();

		public float gizmoSizeFactor = 1;

		[Animate] public float Factor = 1;

		private Line Interpolate(Line g0, Line g1, float t)
		{
			return new Line();
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
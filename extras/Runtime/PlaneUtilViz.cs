using System;
using Needle.Timeline;
using UnityEngine;

namespace _Sample
{
	[ExecuteAlways]
	public class PlaneUtilViz : MonoBehaviour
	{
		public bool Freeze;

		private Ray ray;
		private Vector3? point;
		private Plane plane;
		private float dist;

		private void Start()
		{
			Freeze = false;
		}

		private void OnDrawGizmos()
		{
			if (Event.current.button == 1)
				Freeze = !Freeze;
			if (!Freeze)
			{
				point = PlaneUtils.GetPointOnPlane(Camera.current, out ray, out plane, out dist);
			}

			Gizmos.color = Color.cyan;
			Gizmos.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.LookRotation(plane.normal), new Vector3(1, 1f, .01f));
			Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
			if (point != null)
			{
				var pt = point.Value;
				Gizmos.matrix = Matrix4x4.identity;
				Gizmos.DrawLine(ray.origin, pt);
				Gizmos.DrawSphere(pt, .01f * dist);
			}
		}
	}
}
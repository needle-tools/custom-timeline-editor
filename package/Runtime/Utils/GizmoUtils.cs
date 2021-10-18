using UnityEngine;

namespace Needle.Timeline
{
	public static class GizmoUtils
	{
		public static void DrawArrow(Vector3 start, Vector3 end)
		{
			Gizmos.DrawLine(start, end);
			var dir = end - start;
			var ort = Vector3.Cross(dir * .1f, Vector3.forward);
			Gizmos.DrawLine(end, Vector3.Lerp(start, end + ort, .9f));
			ort *= -1;
			Gizmos.DrawLine(end, Vector3.Lerp(start, end + ort, .9f));
		}
	}
}
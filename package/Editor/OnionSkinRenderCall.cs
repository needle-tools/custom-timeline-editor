using UnityEditor;
using UnityEngine;

namespace Needle.Timeline
{
	internal class OnionSkinRenderCall : Editor
	{
		[DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
		private static void DrawGizmos(Transform  component, GizmoType gizmoType)
		{
		}
	}
}
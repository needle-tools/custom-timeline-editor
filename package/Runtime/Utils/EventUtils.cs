using UnityEditor;
using UnityEngine;

namespace Needle.Timeline
{
	public static class EventUtils
	{
		public static Vector2 GetCurrentMousePositionBottomTop(this Event evt)
		{
			var mp = evt.mousePosition;
			mp.y = Screen.height - mp.y;
#if UNITY_EDITOR
			if (SceneView.currentDrawingSceneView)
				mp.y -= 40;
#endif
			return mp;
		}
	}
}
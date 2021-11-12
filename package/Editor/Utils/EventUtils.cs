using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	public static class EventUtils
	{
		// [MethodImpl(MethodImplOptions.Synchronized)]
		public static bool IsContextClick(this Event evt, Rect rect)
		{
			if (evt.type == EventType.MouseUp && evt.button == (int)MouseButton.RightMouse)
			{
				if (rect.Contains(evt.mousePosition)) return true;
			}
			return false;
		}
	}
}
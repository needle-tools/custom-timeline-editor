using UnityEditor;
using UnityEngine;

namespace Needle.Timeline
{
	public abstract class CustomClipTool : CustomClipToolBase
	{
		protected override void OnInput(EditorWindow window)
		{
			switch (Event.current.type, Event.current.modifiers, Event.current.button)
			{
				case (EventType.ScrollWheel, EventModifiers.Alt, _):
					UseEvent();
					break;

				case (EventType.MouseDown, EventModifiers.None, 0):
					if (OnMouseDown())
						UseEvent();
					break;

				case (EventType.MouseDrag, EventModifiers.None, 0):
					if(OnDrag())
						UseEvent();
					break;
			}
		}

		protected virtual bool OnMouseDown()
		{
			return false;
		}

		protected virtual bool OnDrag()
		{
			return false;
		}
	}
}
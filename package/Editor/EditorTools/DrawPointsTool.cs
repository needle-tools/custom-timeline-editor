using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

namespace Needle.Timeline
{
	public class DrawPointsTool : CustomClipToolBase
	{
		private List<Vector3> currentList;
		
		protected override void OnToolGUI()
		{
			// var pos = GetCurrentMousePositionInScene();
			var pos = PlaneUtils.GetPointOnPlane(Camera.current, out _, out _, out _);
			Handles.DrawWireDisc(pos, Vector3.up, 0.5f);
			
			if (Event.current.button == 0 && Event.current.modifiers == EventModifiers.None)
			{
				switch (Event.current.type)
				{
					case EventType.MouseDown:
						if (ActiveClip is ICustomClip<List<Vector3>> pointsClip)
						{
							if (currentList == null)
							{
								currentList = new List<Vector3>() { Vector3.zero };
								pointsClip.Add(new CustomKeyframe<List<Vector3>>(currentList, (float)CurrentTime));
							}
						}
						UseEvent();
						break;
						
					case EventType.MouseDrag:
						if (currentList != null)
						{
							currentList.Add(Random.insideUnitSphere + pos);
							UseEvent();
						}
						break;
				}
			}

			void UseEvent()
			{
				GUIUtility.hotControl = 0;
				Event.current.Use();
			}
		}
	}
}
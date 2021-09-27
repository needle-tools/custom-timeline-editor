using System;
using System.Collections.Generic;
using System.Net;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Needle.Timeline
{
	public class DrawPointsTool : CustomClipToolBase
	{
		private ICustomKeyframe<List<Vector3>> keyframe;
		
		public override bool Supports(Type type)
		{
			return typeof(List<Vector3>).IsAssignableFrom(type);
		}

		public override void OnActivated()
		{
			base.OnActivated();
			keyframe = null;
		}

		protected override void OnToolInput()
		{
			// var pos = GetCurrentMousePositionInScene();
			var pos = PlaneUtils.GetPointOnPlane(Camera.current, out _, out _, out _);
			Handles.DrawWireDisc(pos, Vector3.up, 0.5f);
			
			if (Event.current.button == 0 && Event.current.modifiers == EventModifiers.None)
			{
				switch (Event.current.type)
				{
					case EventType.MouseDown:
						if (ActiveClip is ICustomClip<List<Vector3>> clip)
						{
							var time = (float)CurrentTime;
							var closest = clip.GetClosest(time);
							if (closest != null &&  Mathf.Abs(time - closest.time) < .1f && closest is ICustomKeyframe<List<Vector3>> kf)
							{
								keyframe = kf;
							}
							if (keyframe == null || Mathf.Abs(time - keyframe.time) > .1f)
							{
								keyframe = new CustomKeyframe<List<Vector3>>(new List<Vector3>(), time);
								clip.Add(keyframe);
							}
						}
						UseEvent(); 
						break;
						
					case EventType.MouseDrag:
						if (keyframe != null)
						{
							keyframe.value.Add(Random.insideUnitSphere + pos);
							keyframe.RaiseValueChangedEvent();
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
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace Needle.Timeline
{
	public class DrawPointsTool : CustomClipToolBase
	{
		private ICustomKeyframe<List<Vector3>> keyframe;
		
		protected override bool OnSupports(Type type)
		{
			return typeof(List<Vector3>).IsAssignableFrom(type);
		}

		protected override void OnAttach(VisualElement element)
		{
			base.OnAttach(element);
			keyframe = null;
		}

		protected override void OnDetach(VisualElement element)
		{
			base.OnDetach(element);
		}

		protected override void OnTool(EditorWindow window)
		{
			// var pos = GetCurrentMousePositionInScene();
			var pos = PlaneUtils.GetPointOnPlane(Camera.current, out _, out _, out _);
			Handles.DrawWireDisc(pos, Vector3.up, 0.5f);
			Handles.DrawWireDisc(pos, Vector3.forward, 0.5f);
			Handles.DrawWireDisc(pos, Vector3.right, 0.5f);

			switch (Event.current.type)
			{
				case EventType.KeyDown:
					break;
			}

			if (Event.current.button == 0 && Event.current.modifiers == EventModifiers.None)
			{
				switch (Event.current.type)
				{
					case EventType.MouseDown:
						var active = Targets.LastOrDefault();
						if (active.IsNull()) return;
						// foreach (var active in Targets)
						{
							// if (!active.ViewModel.currentlyInClipTime) continue;
							if (active.Clip is ICustomClip<List<Vector3>> clip)
							{
								var time = (float)active.ViewModel.clipTime;
								var closest = clip.GetClosest(time);
								if (closest != null &&  Mathf.Abs(time - closest.time) < .1f && closest is ICustomKeyframe<List<Vector3>> kf)
								{
									keyframe = kf;
								}
								if (keyframe == null || Mathf.Abs(time - keyframe.time) > .1f)
								{
									keyframe = new CustomKeyframe<List<Vector3>>(new List<Vector3>(), time);
									CustomUndo.Register(new CreateKeyframe(keyframe, clip));
								}
							}
						}
						UseEvent(); 
						break;
						
					case EventType.MouseDrag:
						if (keyframe != null)
						{
							keyframe.value.Add(Random.insideUnitSphere * .5f + pos);
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
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Constraints;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace Needle.Timeline
{
	public class PaintTool : CustomClipToolBase
	{
		private ICustomKeyframe<List<Vector3>> keyframe;

		private float radius = .5f;
		private bool erase = false;

		protected override bool OnSupports(Type type)
		{
			return typeof(List<Vector3>).IsAssignableFrom(type);
		}

		protected override void OnAttach(VisualElement element)
		{
			base.OnAttach(element);
			keyframe = null;
			element.Add(new Button(() => { erase = !erase; }) { text = "Toggle Erase" });

			foreach (var mod in ToolModule.Modules)
				Debug.Log(mod);
		}

		protected override void OnInput(EditorWindow window)
		{
			var pos = PlaneUtils.GetPointOnPlane(Camera.current, out _, out _, out _);
			Handles.color = erase ? Color.red : Color.white;
			Handles.DrawWireDisc(pos, Vector3.up, radius);
			Handles.DrawWireDisc(pos, Vector3.forward, radius);
			Handles.DrawWireDisc(pos, Vector3.right, radius);

			switch (Event.current.type)
			{
				case EventType.KeyDown:
					switch (Event.current.keyCode)
					{
						case KeyCode.Escape:
							this.Deselect();
							break;
						case KeyCode.W:
							erase = false;
							UseEvent();
							break;
						case KeyCode.E:
							erase = true;
							UseEvent();
							break;
					}
					break;
			}

			switch (Event.current.type, Event.current.modifiers, Event.current.button)
			{
				case (EventType.ScrollWheel, EventModifiers.Alt, _):
					radius += Event.current.delta.y * -.01f;
					radius = Mathf.Clamp(radius, .001f, 20f);
					UseEvent();
					break;

				case (EventType.MouseDown, EventModifiers.None, 0):
					var active = Targets.LastOrDefault();
					if (active.IsNull()) return;
					// foreach (var active in Targets)
					// if (!active.ViewModel.currentlyInClipTime) continue;
					if (active.Clip is ICustomClip<List<Vector3>> clip)
					{
						var time = (float)active.ViewModel.clipTime;
						var closest = clip.GetClosest(time);
						if (closest != null && Mathf.Abs(time - closest.time) < .1f && closest is ICustomKeyframe<List<Vector3>> kf)
						{
							keyframe = kf;
						}
						if (!erase && (keyframe == null || Mathf.Abs(time - keyframe.time) > .1f))
						{
							keyframe = new CustomKeyframe<List<Vector3>>(new List<Vector3>(), time);
							CustomUndo.Register(new CreateKeyframe(keyframe, clip));
						}
					}
					UseEvent();
					break;

				case (EventType.MouseDrag, EventModifiers.None, 0):
					if (keyframe != null)
					{
						if (erase)
						{
							if (keyframe.value.Count > 0 && RemoveInRange(pos, keyframe.value))
							{
								keyframe.RaiseValueChangedEvent();
							}
						}
						else
						{
							keyframe.value.Add(GetPoint(pos));
							keyframe.RaiseValueChangedEvent();
						}
					}
					UseEvent();
					break;
				
				case (EventType.MouseUp, EventModifiers.None, 0):
					TimelineBuffer.RequestBufferCurrentInspectedTimeline();
					break;
			}
		}

		private bool RemoveInRange(Vector3 pos, IList<Vector3> points)
		{
			var cnt = points.Count;
			for (var index = 0; index < points.Count; index++)
			{
				var pt = points[index];
				if (Vector3.Distance(pt, pos) <= radius)
				{
					points.RemoveAt(index);
				}
			}
			return cnt > points.Count;
		}

		private Vector3 GetPoint(Vector3 pos)
		{
			var pt = Random.insideUnitSphere * radius + pos;
			if (IsIn2DMode)
				pt.z = 0;
			return pt;
		}
	}
}
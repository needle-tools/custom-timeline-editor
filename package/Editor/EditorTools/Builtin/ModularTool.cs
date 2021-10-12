using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace Needle.Timeline
{
	public class ModularTool : CustomClipToolBase
	{
		private ICustomKeyframe keyframe;

		private float radius = .5f;

		protected override bool OnSupports(Type type)
		{
			if (typeof(IList).IsAssignableFrom(type))
			{
				if (type.IsGenericType) 
				{
					var content = type.GetGenericArguments().FirstOrDefault();
					if (content != null)
					{
						foreach (var field in content.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
						{
							Debug.Log(field.DeclaringType + ": " +  field.Name);
						}
					}
				}
			}
			else
			{
				
			}
			return typeof(List<Vector3>).IsAssignableFrom(type);
		}

		protected override void OnAttach(VisualElement element)
		{
			base.OnAttach(element);
			keyframe = null;
			// element.Add(new Button(() => { erase = !erase; }) { text = "Toggle Erase" });

			foreach (var mod in ToolModule.Modules)
				Debug.Log(mod);
		}

		protected override void OnInput(EditorWindow window)
		{
			var pos = PlaneUtils.GetPointOnPlane(Camera.current, out _, out _, out _);
			// Handles.color = erase ? Color.red : Color.white;
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
							// erase = false;
							// UseEvent();
							break;
						case KeyCode.E:
							// erase = true;
							// UseEvent();
							break;
					}
					break;

				// case EventType.MouseDown:
				// 	ForEachModule((toolTarget, module) => module.BeginInput(toolTarget));
				// 	break;
				// case EventType.MouseDrag:
				// 	ForEachModule((toolTarget, module) => module.UpdateInput(toolTarget));
				// 	break;
				// case EventType.MouseUp:
				// 	ForEachModule((toolTarget, module) => module.EndInput(toolTarget));
				// 	break;
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
					if (active.IsNull() == false)
					{
						var time = (float)active.ViewModel.clipTime;
						var closest = active.Clip.GetClosest(time);
						if (closest != null && Mathf.Abs(time - closest.time) < .1f)
						{
							keyframe = closest;
						}
						if (keyframe == null || Mathf.Abs(time - keyframe.time) > .1f)
						{
							if (active.Clip.GetType().IsGenericType)
							{
								var clipType = active.Clip.GetType().GetGenericArguments().FirstOrDefault();
								var keyframeType = typeof(CustomKeyframe<>).MakeGenericType(clipType);
								keyframe = Activator.CreateInstance(keyframeType) as ICustomKeyframe;
								keyframe!.time = time;
								CustomUndo.Register(new CreateKeyframe(keyframe, active.Clip));
							}
						}
					}
					UseEvent();
					break;

				case (EventType.MouseDrag, EventModifiers.None, 0):
					if (keyframe != null)
					{
						// keyframe.value.Add(GetPoint(pos));
						keyframe.RaiseValueChangedEvent();
					}
					UseEvent();
					break;
			}
		}

		private void ForEachModule(Action<ToolTarget, ToolModule> callback)
		{
			foreach (var t in Targets)
			{
				foreach (var mod in ToolModule.Modules)
				{
					callback(t, mod);
				}
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
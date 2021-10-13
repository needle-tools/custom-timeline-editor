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
	public class ModuleView
	{
		public ToolModule Module;
		public VisualElement Container;
		public TextElement Label;

		private bool active;
		public bool IsActive => active;
		
		public void SetActive(bool state)
		{
			this.active = state;
			Label.style.color = this.active ? Color.white : Color.gray;
			Debug.Log(active);
		}

		public ModuleView(VisualElement container)
		{
			Container = container;
		}

		public bool Is(IToolModule mod)
		{
			return Module == mod;// TargetField.DeclaringType == field.DeclaringType && TargetField.Name == field.Name;
		}
	}
	
	public class ModularTool : CustomClipToolBase
	{
		private ICustomKeyframe keyframe;

		private float radius = .5f;

		protected override bool OnSupports(Type type)
		{
			foreach (var field in type.EnumerateFields())
			{
				if (ToolModule.Modules.Any(m => m.CanModify(field)))
				{
					return true;
				}
			}
			return typeof(List<Vector3>).IsAssignableFrom(type);
		}

		protected override void OnAttach(VisualElement element)
		{
			base.OnAttach(element);
			keyframe = null;
			modulesContainer ??= new VisualElement();
			element.Add(modulesContainer);
		}

		private static readonly List<IToolModule> buffer = new List<IToolModule>();
		private VisualElement modulesContainer;
		private List<ModuleView> modulesUI = new List<ModuleView>();


		protected override void OnAddedTarget(ToolTarget _)
		{
			base.OnAddedTarget(_);
			OnTargetsChanged();
		}

		protected override void OnRemovedTarget(ToolTarget t)
		{
			base.OnRemovedTarget(t);
			OnTargetsChanged();
		}

		private void OnTargetsChanged()
		{
			//modulesContainer.Clear();
			foreach (var t in Targets)
			{
				foreach (var field in t.Clip.EnumerateFields())
				{
						
					ToolModule.GetModulesSupportingType(field, buffer);
					if (buffer.Count > 0)
					{
						VisualElement container = null;
						foreach (var mod in buffer)
						{
							var entry = modulesUI.FirstOrDefault(e => e.Is(mod));
							if (entry != null) continue;

							if (container == null)
							{
								container = new VisualElement();
								modulesContainer.Add(container);
								// var label = new Label(field.Name + " : " + field.FieldType.Name);
								// container.Add(label);
							}
								
							entry = new ModuleView(modulesContainer);
							entry.Module = (ToolModule)mod;
							modulesUI.Add(entry);

							Button button = null;
							button = new Button(() =>
							{
								foreach (var e in modulesUI)
								{
									if (e == entry) continue;
									e.SetActive(false);
								}
								entry.SetActive(!entry.IsActive);
							})
							{
								text = mod.GetType().Name
							};
							entry.Label = button;
							container.Add(button);
							entry.SetActive(false);
						}
					}
				}
			}
		}

		private readonly ToolInputData data = new ToolInputData();
		private readonly List<(IReadClipTime time, IClipKeyframePair val)> visibleKeyframes = new List<(IReadClipTime time, IClipKeyframePair val)>();

		protected override void OnInput(EditorWindow window)
		{
			if (Event.current.type == EventType.MouseDown)
				Debug.Log("Mouse down");
			
			data.Update();
			// foreach (var tool in modulesUI)
			// {
			// 	if (!tool.IsActive) continue;
			// 	foreach (var field in EnumerateTargetFields())
			// 	{
			// 		tool.Module.RequestsInput(data);
			// 	}
			// }


			visibleKeyframes.Clear();
			if (modulesUI.Any(m => m.IsActive && m.Module.WantsInput(data)))
			{
				foreach (var tar in Targets)
				{
					if (!tar.IsNull())
					{
						var time = (float)tar.ViewModel.clipTime;
						var closest = tar.Clip.GetClosest(time);
						if (closest == null) continue;
						if (closest.time > time) continue;
						visibleKeyframes.Add((tar.ViewModel, new ClipKeyframePair(tar.Clip, closest)));
					}
				}
				Debug.Log("KF=" + visibleKeyframes.Count);
				Debug.Log("Modules=" + modulesUI.Count);
				foreach (var (read, pair) in visibleKeyframes)
				{
					var time = (float)read.ClipTime;
					
					var kf = pair?.Keyframe;
					if (kf == null) continue;
					Debug.Log(kf.time);
					foreach (var mod in modulesUI)
					{
						if (mod.IsActive && mod.Module.WantsInput(data))
						{
							mod.Module.OnModify(data, kf.EnumerateFields(), () => kf.value);
						}
					}
					
					if (pair != null && Mathf.Abs(time - pair.Keyframe.time) < .1f)
					{
					}
					if (keyframe == null || Mathf.Abs(time - keyframe.time) > .1f)
					{
					}
				}
				UseEvent();
			}
			// var pos = PlaneUtils.GetPointOnPlane(Camera.current, out _, out _, out _);
			// // Handles.color = erase ? Color.red : Color.white;
			// Handles.DrawWireDisc(pos, Vector3.up, radius);
			// Handles.DrawWireDisc(pos, Vector3.forward, radius);
			// Handles.DrawWireDisc(pos, Vector3.right, radius);

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
					// foreach (var active in Targets)
					// if (!active.ViewModel.currentlyInClipTime) continue;
					if (!active.IsNull())
					{
						var time = (float)active.ViewModel.clipTime;
						var closest = active.Clip.GetClosest(time);
						if (closest != null && Mathf.Abs(time - closest.time) < .1f)
						{
							keyframe = closest;
						}
						if (keyframe == null || Mathf.Abs(time - keyframe.time) > .1f)
						{
							// if (active.Clip.GetType().IsGenericType)
							// {
							// 	var clipType = active.Clip.GetType().GetGenericArguments().FirstOrDefault();
							// 	var keyframeType = typeof(CustomKeyframe<>).MakeGenericType(clipType);
							// 	keyframe = Activator.CreateInstance(keyframeType) as ICustomKeyframe;
							// 	keyframe!.time = time;
							// 	CustomUndo.Register(new CreateKeyframe(keyframe, active.Clip));
							// }
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
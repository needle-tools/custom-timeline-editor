﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace Needle.Timeline
{
	public class ModularTool : CustomClipToolBase
	{
		protected override bool OnSupports(Type type)
		{
			return ToolModuleRegistry.Modules.Any(m => m.CanModify(type));
		}

		protected override void OnAttach(VisualElement element)
		{
			base.OnAttach(element);
			modulesContainer ??= new VisualElement();
			element.Add(modulesContainer);
		}

		private static readonly List<IToolModule> buffer = new List<IToolModule>();
		private VisualElement modulesContainer;
		private readonly List<ModuleViewController> moduleViewControllers = new List<ModuleViewController>();


		protected override void OnAddedTarget(ToolTarget t)
		{
			base.OnAddedTarget(t);
			OnTargetsChanged();
		}

		protected override void OnRemovedTarget(ToolTarget t)
		{
			base.OnRemovedTarget(t);
			OnTargetsChanged();
		}

		protected override void OnRecordingStateChanged(IRecordable obj)
		{
			foreach (var view in moduleViewControllers)
			{
				view.OnTargetsChanged();
			}
		}

		private void OnTargetsChanged()
		{
			foreach (var t in Targets)
			{
				if (t.Clip == null) continue;
				foreach (var type in t.Clip.SupportedTypes)
				{ 
					ToolModuleRegistry.GetModulesSupportingType(type, buffer);
					BuildModuleToolsUI();

					if (typeof(ICollection).IsAssignableFrom(type) && type.IsGenericType)
					{
						var contentType = type.GetGenericArguments().First();
						ToolModuleRegistry.GetModulesSupportingType(contentType, buffer);
						BuildModuleToolsUI();
					}
				}

				foreach (var field in t.Clip.EnumerateFields())
				{
					ToolModuleRegistry.GetModulesSupportingType(field.FieldType, buffer);
					BuildModuleToolsUI();
				}
			}

			foreach (var view in moduleViewControllers)
			{
				view.OnTargetsChanged();
			}
		}

		private void BuildModuleToolsUI()
		{
			if (buffer.Count <= 0) return;

			foreach (var module in buffer)
			{
				var entry = moduleViewControllers.FirstOrDefault(e => e.Is(module));
				if (entry != null) continue;
				var type = module.GetType();
				var meta = type.GetCustomAttribute<Meta>();

				var container = new VisualElement();
				modulesContainer.Add(container);

				entry = new ModuleViewController(modulesContainer, module, this);
				moduleViewControllers.Add(entry);

				Button button = null;
				button = new Button(() =>
				{
					foreach (var e in moduleViewControllers)
					{
						if (e == entry) continue;
						e.SetActive(false);
					}
					entry.SetActive(!entry.IsActive);
				});
				button.text = ObjectNames.NicifyVariableName(meta?.Name ?? module.GetType().Name);
				entry.Label = button;
				container.Add(button);
				entry.SetActive(false);
			}
		}

		private readonly InputData input = new InputData();


		protected override InputEventStage OnHandleInput()
		{
			base.OnHandleInput();
			input.Update();
			
			if (moduleViewControllers.Any(m => m.IsActive))
			{
				if(Event.current.button == 0 && Event.current.modifiers == EventModifiers.None && !Event.current.isScrollWheel)
					UseEventDelayed();
				
				foreach (var mod in moduleViewControllers)
				{
					if (!mod.IsActive) continue;
					var module = mod.Module;
					if (!mod.Module.WantsInput(input)) continue;

					foreach (var tar in Targets)
					{
						if (tar.IsNull()) continue;
						if (tar.Clip is IRecordable { IsRecording: false }) continue;
						var sup = tar.Clip!.SupportedTypes.Any(module.CanModify);
						if (sup)
						{
							var data = new ToolData(tar.Object, tar.Clip, tar.TimeF, tar, CommandHandler);
							tar.EnsurePaused();
							module.OnModify(input, ref data);
							UseEventDelayed();
						}
					}
				}
			}
			
			switch (Event.current.type)
			{
				case EventType.KeyDown:
					switch (Event.current.keyCode)
					{
						case KeyCode.Escape:
							this.Deselect();
							return input.Stage;
					}
					break;
			}

			return input.Stage;
		}
		
		
		[DrawGizmo(GizmoType.NonSelected | GizmoType.Selected)]
		private static void DrawGizmos(PlayableDirector component, GizmoType gizmoType)
		{
			foreach (var tools in ToolsHandler.ActiveTools)
			{
				if (tools is ModularTool mod)
				{
					if (mod.moduleViewControllers == null) continue;
					if (mod.moduleViewControllers.Any(m => m.IsActive))
					{
						mod.input.Update();
						foreach (var view in mod.moduleViewControllers)
						{
							if (!view.IsActive) continue;
							var module = view.Module;
							module.OnDrawGizmos(mod.input);
						}
					}
				}
			}
		}

	}
}
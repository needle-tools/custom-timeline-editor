using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.AccessControl;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	public class ModuleViewController
	{
		public readonly IToolModule Module;
		private readonly ICustomClipTool tool;
		public TextElement Label;

		private bool active;
		public bool IsActive => active;

		private bool initOptions;
		private readonly VisualElement options;
		private readonly VisualElement bindingsContainer;
		private readonly List<IViewFieldBinding> bindings = new List<IViewFieldBinding>();

		public void SetActive(bool state)
		{
			this.active = state;
			Label.style.color = this.active ? Color.white : Color.gray;

			options.style.display = new StyleEnum<DisplayStyle>(state ? StyleKeyword.Auto : StyleKeyword.None);
			options.style.visibility = state ? Visibility.Visible : Visibility.Hidden;

			// options.SetEnabled(state);
		}

		public ModuleViewController(VisualElement container, IToolModule module, ICustomClipTool tool)
		{
			this.Module = module;
			this.tool = tool;

			options = new VisualElement();
			container.Add(options);
			bindingsContainer = new VisualElement();
			OnBuildUI();
		}

		public bool Is(IToolModule mod)
		{
			return Module == mod; // TargetField.DeclaringType == field.DeclaringType && TargetField.Name == field.Name;
		}

		public void OnTargetsChanged()
		{
			OnBuildUI();
		}

		private void OnBuildUI()
		{
			options.Clear();
			
			options.Add(new Label(Module.GetType().Name)
			{
				style = { unityFontStyleAndWeight = FontStyle.Bold}
			});

			var addedModuleField = false;
			foreach (var field in Module.GetType().EnumerateFields())
			{
				if (!field.IsPublic)
				{
					if (field.GetCustomAttribute<Expose>() == null)
						continue;
				}
				
				if (!addedModuleField)
				{
					// options.Add(new Label("Toggle at start doesnt do anything right now and will be removed"));
				}

				addedModuleField = true;
				var binding = field.BuildControl(Module, true);
				options.Add(binding.ViewElement);
			}
			
			if (Module is IBindsFields bindable && bindable.AllowBinding)
			{
				bindable.Bindings.Clear();
				
				foreach (var t in tool.Targets)
				{
					
					// bindingsContainer.Add(new Label(headerText));
					if (t.Clip == null) continue;
					foreach (var field in t.Clip.EnumerateFields())
					{
						// if (BindingsCache.TryGetFromCache(field, out var c))
						// {
						// 	options.Add(c.ViewElement);
						// 	continue;
						// }
							
						if (ControlsFactory.TryBuildBinding(field, t, bindable, out var handler))
						{
							options.Add(handler.ViewElement);
						}
					}
				}
			}
			ToolsWindow.Root.Add(options);
		}
	}
}
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.AccessControl;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
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
		private readonly ScrollView options;
		private readonly Foldout foldout;

		public void SetActive(bool state)
		{
			this.active = state;
			Label.style.color = this.active ? Color.white : Color.gray;

			options.style.display = new StyleEnum<DisplayStyle>(state ? StyleKeyword.Auto : StyleKeyword.None);
			options.style.visibility = state ? Visibility.Visible : Visibility.Hidden;
			
			OnBuildUI();
		}

		public ModuleViewController(VisualElement container, IToolModule module, ICustomClipTool tool)
		{
			this.Module = module;
			this.tool = tool;

			options = new ScrollView();
			container.Add(options);

			foldout = new Foldout();
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
			options.style.paddingTop = new StyleLength(new Length(10, LengthUnit.Pixel));
			
			// options.Add(new Label(Module.GetType().Name)
			// {
			// 	style = { unityFontStyleAndWeight = FontStyle.Bold}
			// });

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
				var binding = field.BuildControl(Module, true, false);
				options.Add(binding.ViewElement);
			}
			
			if (IsActive && Module is IBindsFields bindable && bindable.AllowBinding)
			{
				bindable.Bindings.Clear();
				options.Add(new VisualElement(){style = { height = 20}});
				
				foreach (var t in tool.Targets)
				{
					var clip = t.Clip;
					if (clip == null) continue;
					if (t.Clip is IRecordable rec && !rec.IsRecording) continue;

					foreach (var field in clip.EnumerateFields())
					{
						if (BindingsCache.TryGetFromCache(field, out var c))
						{
							bindable.Bindings.Add(c);
							options.Add(c.ViewElement);
							continue;
						}
						// if (BindingsCache.TryGetFromCache(field, out var c))
						// {
						// 	options.Add(c.ViewElement);
						// 	continue;
						// }
							
						if (ControlsFactory.TryBuildBinding(field, out var handler))
						{
							Debug.Log("BUILT: " + handler.GetHashCode() + ", " + clip.Name + ":" + field.Name);
							bindable.Bindings.Add(handler); 
							options.Add(handler.ViewElement);
						}
					}
				}
			}
			
			ToolsWindow.Root.Add(options);
		}
	}
}
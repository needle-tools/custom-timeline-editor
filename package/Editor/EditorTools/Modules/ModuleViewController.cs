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

			bindingsContainer.style.display = state ? DisplayStyle.Flex : DisplayStyle.None;
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
			// options.Add(new Button(OnBuildUI) { text = "DEBUG REBUILD UI" });

			foreach (var field in Module.GetType().EnumerateFields())
			{
				if (!field.IsPublic)
				{
					if (field.GetCustomAttribute<Expose>() == null)
						continue;
				}

				var range = field.GetCustomAttribute<RangeAttribute>();
				VisualElement element = null;
				const float labelMinWidth = 40;

				// new MockBinding(field.Name, field.GetValue(Module), true, range).BuildControl(options);

				if (field.FieldType == typeof(float))
				{
					if (range != null)
					{
						var el = new Slider(field.Name);
						CreateSlider(el);
					}
					else
					{
						var el = new FloatField(field.Name);
						CreateTextField(el);
					}
				}
				else if (field.FieldType == typeof(int))
				{
					if (range != null)
					{
						var el = new Slider(field.Name);
						CreateSlider(el);
					}
					else
					{
						var el = new IntegerField(field.Name);
						CreateTextField(el);
					}
				}
				else if (field.FieldType == typeof(bool))
				{
					var el = new Toggle(field.Name);
					CreateToggle(el);
				}
				else
				{
					var label = new Label();
					label.text = field.Name;
					options.Add(label);
				}

				element?.RegisterCallback<MouseUpEvent>(evt =>
				{
					if (evt.button == (int)MouseButton.RightMouse)
					{
						var menu = new GenericMenu();
						menu.AddItem(new GUIContent("Test"), false, f => { Debug.Log("OK"); }, null);
						menu.ShowAsContext();
					}
				}); 

				void CreateSlider<T>(BaseSlider<T> slider) where T : IComparable<T>
				{
					slider.Q<Label>().style.minWidth = labelMinWidth;
					slider.style.minWidth = 200;
					slider.lowValue = (T)Convert.ChangeType(range.min, typeof(float));
					slider.highValue = (T)Convert.ChangeType(range.max, typeof(float));
					slider.value = (T)Convert.ChangeType(field.GetValue(Module), typeof(T));
					slider.RegisterValueChangedCallback(cb => { field.SetValue(Module, cb.newValue); });
					options.Add(slider);
					element = slider;
				}

				void CreateTextField<T>(TextValueField<T> el)
				{
					el.Q<Label>().style.minWidth = labelMinWidth;
					el.value = (T)Convert.ChangeType(field.GetValue(Module), typeof(T));
					el.RegisterValueChangedCallback(cb => { field.SetValue(Module, Convert.ChangeType(cb.newValue, field.FieldType)); });
					element = el;
					options.Add(el);
				}

				void CreateToggle(Toggle el)
				{
					el.Q<Label>().style.minWidth = labelMinWidth;
					el.value = (bool)field.GetValue(Module);
					el.RegisterValueChangedCallback(cb => { field.SetValue(Module, cb.newValue); });
					element = el;
					options.Add(el);
				}
			}

			// options.Add(bindingsContainer);
			// bindingsContainer.Clear();

			if (Module is IBindsFields bindable && bindable.AllowBinding)
			{
				bindable.Bindings.Clear();
				
				foreach (var t in tool.Targets)
				{
					Foldout foldout = null;
					
					// bindingsContainer.Add(new Label(headerText));
					foreach (var field in t.Clip.EnumerateFields())
					{ 
						if (ControlsFactory.TryBuildBinding(this, field, t, bindable, out var handler))
						{
							if (foldout == null)
							{
								var headerText = ObjectNames.NicifyVariableName(t.Clip.Name);
								foldout = new Foldout() { text = headerText }; 
								options.Add(foldout);
							}
							foldout.Add(handler.ViewElement);
						}
					}
				}
			}
			ToolsWindow.Root.Add(options);
		}
	}
}
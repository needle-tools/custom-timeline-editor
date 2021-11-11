#nullable enable
using System;
using System.Reflection;
using Needle.Timeline.AssetBinding;
using UnityEditor;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	internal static class ControlsFactory
	{
		[BindAsset("a9727f46214640d1be592eb4e81682ee")]
		private static VisualTreeAsset? controlAsset;
		[BindAsset("e29516eda36d4ad1b6f8822975c7f21c")]
		private static StyleSheet? controlStyles;
		
		[BindAsset("907bae41c16d4edcbfd166200df5be05")]
		private static VisualTreeAsset? toolsPanel;
		[BindAsset("e1df1297d0a64f8185b0bf8e4c55c5a0")] 
		private static StyleSheet? toolsPanelStyles;  

		public static bool TryBuildToolPanel(out VisualElement panel, bool preview = false)
		{
			panel = toolsPanel!.CloneTree().contentContainer;
			panel.styleSheets.Add(toolsPanelStyles);

			if (!preview)
			{
				foreach (var p in panel.Query<VisualElement>(null, "template").ToList())
				{
					p.style.display = DisplayStyle.None;
				}
			}

			return true;
		}


		public static bool TryBuildBinding(FieldInfo field, ToolTarget target, IBindsFields bindable, out ViewValueBindingController? res)
		{
			if (field.IsStatic)
			{
				res = null;
				return false;
			}

			PersistenceHelper.TryGetPreviousValue(field, out var currentValue);
			var viewValue = new ViewValueProxy(currentValue);
			viewValue.ValueChanged += newValue =>
			{
				PersistenceHelper.OnValueChanged(field, newValue);
			};
			res = new ViewValueBindingController(field, viewValue, target.Clip);
			res.ViewElement = res.BuildControl();
			res.Init();
			bindable.Bindings.Add(res);
			return res.ViewElement != null;
		}

		public static IViewFieldBinding BuildControl(this FieldInfo field, object instance, VisualElement? target = null, bool? enabled = null)
		{
			var viewBinding = new FieldViewBinding(instance, field);
			var controller = new ViewValueBindingController(field, viewBinding, instance as IRecordable);
			if (enabled != null)
				controller.Enabled = enabled.Value;
			BuildControl(controller, target);
			return controller;
		}

		public static VisualElement BuildControl(this IViewFieldBinding binding, VisualElement? target = null)
		{
			if (TryBuildControl(binding.ValueType, binding, out var control))
			{
				var instance = controlAsset!.CloneTree();
				instance.styleSheets.Add(controlStyles);


				var labelText = ObjectNames.NicifyVariableName(binding.Name); // CultureInfo.CurrentCulture.TextInfo.ToTitleCase(binding.Name);
				var name = instance.Q<Label>(null, "control-label");
				if (name != null)
					name.text = labelText;

				// try move the label out of the created control label and replace our uxml label with it
				// we do this so we get the drag functionality for free (if an element has any)
				var label = control.Q<Label>(null, "unity-label");
				if (label != null)
				{
					// label.RegisterCallback(new EventCallback<MouseManipulator>(e =>{}));
					label.AddToClassList("control-label");
					label.text = labelText;
					if (name != null)
					{
						name.parent.Insert(name.parent.IndexOf(name), label);
						name.RemoveFromHierarchy();
					}
				}
				else label = name;
				//
				// if (typeof(int).IsAssignableFrom(binding.ValueType))
				// {
				// 	var dragger = (BaseFieldMouseDragger)new FieldMouseDragger<int>(control.Q<IntegerField>());
				// 	
				// }
				
				var controlContainer = instance.Q<VisualElement>(null, "control");
				binding.ViewElement = control;
				controlContainer.Add(control);


				var toggle = instance.Q<Toggle>(null, "enabled");
				toggle.RegisterValueChangedCallback(evt =>
				{
					binding.Enabled = evt.newValue;
					UpdateViews(evt.newValue);
				});
				binding.EnabledChanged += UpdateViews;
				UpdateViews(binding.Enabled);

				void UpdateViews(bool enabled)
				{
					toggle.SetValueWithoutNotify(enabled);
					controlContainer.SetEnabled(enabled);
					label?.SetEnabled(enabled);
				}

				if (target != null)
					target.Add(instance);
				return instance;
			}

			var missingLabel = new Label("Missing " + binding.ValueType);
			if (target != null) target.Add(missingLabel);
			return missingLabel;
		}

		private static readonly ImplementorsRegistry<IControlBuilder> builders = new ImplementorsRegistry<IControlBuilder>();

		private struct Context : IContext
		{
			public Context(IHasCustomAttributes? attributes)
			{
				Attributes = attributes;
			}

			public IHasCustomAttributes? Attributes { get; }
		}

		private static bool TryBuildControl(Type type, IViewFieldBinding binding, out VisualElement? control)
		{
			if (builders.TryGetInstance(i => i.CanBuild(type), out var match))
			{
				control = match.Build(type, binding.ViewValue, new Context(binding)); 
				return control != null;
			}
			control = null;
			return false;
		}
	}
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	internal static class ControlsFactory
	{
		private static void Init()
		{
			if (isInit) return;
			isInit = true;
			controlAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath("a9727f46214640d1be592eb4e81682ee"));
			controlStyles = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath("e29516eda36d4ad1b6f8822975c7f21c"));
		}

		private static bool isInit;
		private static VisualTreeAsset controlAsset;
		private static StyleSheet controlStyles;

		
		public static bool TryBuildBinding(ModuleViewController viewController, FieldInfo field, ToolTarget target, IBindsFields bindable, out ClipFieldBindingController res)
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
			res = new ClipFieldBindingController(target.Clip, field, viewValue);
			res.ViewElement = res.BuildControl();
			res.Init();
			bindable.Bindings.Add(res);
			// res = binding; 
			// CreateFieldView(field, binding);
			//
			// if (binding.ViewElement == null)
			// {
			// 	Debug.LogWarning("Did not find handler for " + field.FieldType);
			// 	return false;
			// }
			//
			// binding.Enabled = true;
			// bindable.Bindings.Add(binding);
			return res.ViewElement != null;
		}

		public static VisualElement BuildControl(this IViewFieldBinding binding, VisualElement target = null)
		{
			Init();
			
			if (TryBuildControl(binding.ValueType, binding, out var control))
			{
				var instance = controlAsset.CloneTree();
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

		private static bool TryBuildControl(Type type, IViewFieldBinding binding, out VisualElement control)
		{
			var viewValue = binding.ViewValue;

			if (builders.TryGetInstance(i => i.CanBuild(type), out var match))
			{
				control = match.Build(type, binding.ViewValue);
				return control != null;
			}

			if (typeof(int).IsAssignableFrom(type))
			{
				var view = new IntegerField();
				view.RegisterValueChangedCallback(evt => { viewValue.SetValue(evt.newValue); });
				var val = viewValue.GetValue();
				if(val != null)
					view.value = (int)val;
				control = view;
				view.label = "_";

				var range = binding.GetCustomAttribute<RangeAttribute>();
				if (range != null) 
				{
					var sliderContainer = new VisualElement();
					sliderContainer.AddToClassList("control");
					var slider = new SliderInt((int)range.min, (int)range.max);
					slider.value = (int)viewValue.GetValue();
					slider.RegisterValueChangedCallback(evt =>
					{
						view.SetValueWithoutNotify(evt.newValue);
						viewValue.SetValue(evt.newValue);
					});
					view.RegisterValueChangedCallback(evt => { slider.SetValueWithoutNotify(evt.newValue); });

					sliderContainer.Add(view);
					sliderContainer.Add(slider);
					control = sliderContainer;
				}

				return true;
			}

			if (typeof(string).IsAssignableFrom(type))
			{
				var view = new TextField();
				view.label = "_";
				view.RegisterValueChangedCallback(evt => { viewValue.SetValue(evt.newValue); });
				var val = viewValue.GetValue();
				if(val != null)
					view.value = (string)val;

				control = view;
				return true;
			}

			if (typeof(float).IsAssignableFrom(type))
			{
				var view = new FloatField();
				view.label = "_";
				view.RegisterValueChangedCallback(evt => { viewValue.SetValue(evt.newValue); });
				var val = viewValue.GetValue();
				if(val != null)
					view.value = (float)val;
				control = view;

				var range = binding.GetCustomAttribute<RangeAttribute>();
				if (range != null) 
				{
					var sliderContainer = new VisualElement();
					sliderContainer.AddToClassList("control");
					var slider = new Slider(range.min, range.max);
					slider.value = (float)viewValue.GetValue();
					slider.RegisterValueChangedCallback(evt =>
					{
						view.SetValueWithoutNotify(evt.newValue);
						viewValue.SetValue(evt.newValue);
					});
					view.RegisterValueChangedCallback(evt => { slider.SetValueWithoutNotify(evt.newValue); });

					sliderContainer.Add(view);
					sliderContainer.Add(slider);
					control = sliderContainer;
				}

				return true;
			}

			if (typeof(double).IsAssignableFrom(type))
			{
				var view = new DoubleField();
				view.label = "_";
				view.RegisterValueChangedCallback(evt => { viewValue.SetValue(evt.newValue); });
				var val = viewValue.GetValue();
				if(val != null)
					view.value = (double)val;

				control = view;
				return true;
			}

			if (typeof(Vector4).IsAssignableFrom(type))
			{
				var view = new Vector4Field();
				view.label = "_";
				view.RegisterValueChangedCallback(evt => { viewValue.SetValue(evt.newValue); });
				var val = viewValue.GetValue();
				if(val != null)
					view.value = (Vector4)val;

				control = view;
				return true;
			}

			if (typeof(Vector3).IsAssignableFrom(type))
			{
				var view = new Vector3Field();
				view.label = "_";
				view.RegisterValueChangedCallback(evt => { viewValue.SetValue(evt.newValue); });
				var val = viewValue.GetValue();
				if(val != null)
					view.value = (Vector3)val;

				control = view;
				return true;
			}

			if (typeof(Vector3Int).IsAssignableFrom(type))
			{
				var view = new Vector3IntField();
				view.label = "_";
				view.RegisterValueChangedCallback(evt => { viewValue.SetValue(evt.newValue); });
				var val = viewValue.GetValue();
				if(val != null)
					view.value = (Vector3Int)val;

				control = view;
				return true;
			}

			if (typeof(Vector2).IsAssignableFrom(type))
			{
				var view = new Vector2Field();
				view.label = "_";
				view.RegisterValueChangedCallback(evt => { viewValue.SetValue(evt.newValue); });
				var val = viewValue.GetValue();
				if(val != null)
					view.value = (Vector2)val;

				control = view;
				return true;
			}

			if (typeof(Vector2Int).IsAssignableFrom(type))
			{
				var view = new Vector2IntField();
				view.label = "_";
				view.RegisterValueChangedCallback(evt => { viewValue.SetValue(evt.newValue); });
				var val = viewValue.GetValue();
				if(val != null)
					view.value = (Vector2Int)val;

				control = view;
				return true;
			}

			if (typeof(Color).IsAssignableFrom(type))
			{
				var view = new ColorField();
				view.label = "_";
				view.RegisterValueChangedCallback(evt => { viewValue.SetValue(evt.newValue); });
				var val = viewValue.GetValue();
				if(val != null)
					view.value = (Color)val; 
				else
				{
					view.value = Color.white;
					viewValue.SetValue(view.value);
				}
				var usage = binding.GetCustomAttribute<ColorUsageAttribute>();
				if (usage != null)
				{
					view.hdr = usage.hdr;
					view.showAlpha = usage.showAlpha;
				}

				control = view;
				return true;
			}


			control = null;
			return false;
		}
	}
}
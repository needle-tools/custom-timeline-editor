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
		[InitializeOnLoadMethod]
		private static void Init()
		{
			controlAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath("a9727f46214640d1be592eb4e81682ee"));
			controlStyles = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath("e29516eda36d4ad1b6f8822975c7f21c"));
		}

		private static VisualTreeAsset controlAsset;
		private static StyleSheet controlStyles;


		public static VisualElement BuildControl(this IViewFieldBinding binding, VisualElement target = null)
		{
			if (TryBuildControl(binding.ValueType, binding, out var control))
			{
				var instance = controlAsset.CloneTree();
				instance.styleSheets.Add(controlStyles);

				var toggle = instance.Q<Toggle>(null, "enabled");
				toggle.value = binding.Enabled;


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
				// else label = name;
				//
				// if (typeof(int).IsAssignableFrom(binding.ValueType))
				// {
				// 	var dragger = (BaseFieldMouseDragger)new FieldMouseDragger<int>(control.Q<IntegerField>());
				// 	
				// }
				
				var controlContainer = instance.Q<VisualElement>(null, "control");
				binding.ViewElement = control;
				controlContainer.Add(control);

				controlContainer.SetEnabled(toggle.value);
				toggle.RegisterValueChangedCallback(evt => controlContainer.SetEnabled(evt.newValue));

				if (target != null)
					target.Add(instance);
				return instance;
			}

			var missingLabel = new Label("Missing " + binding.ValueType);
			if (target != null) target.Add(missingLabel);
			return missingLabel;
		}

		private static bool TryBuildControl(Type type, IViewFieldBinding binding, out VisualElement control)
		{
			var viewValue = binding.ViewValue;

			if (typeof(Enum).IsAssignableFrom(type))
			{
				var enumOptions = Enum.GetNames(type);
				var view = new PopupField<string>(enumOptions.ToList(), 0);
				view.label = "_";

				view.RegisterValueChangedCallback(evt =>
				{
					var val = (Enum)Enum.Parse(type, evt.newValue);
					viewValue.SetValue(val);
				});
				view.value = viewValue.GetValue().ToString();

				control = view;
				return true;
			}

			if (typeof(int).IsAssignableFrom(type))
			{
				var view = new IntegerField();
				view.RegisterValueChangedCallback(evt => { viewValue.SetValue(evt.newValue); });
				view.value = (int)viewValue.GetValue();
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
				view.value = (string)viewValue.GetValue();

				control = view;
				return true;
			}

			if (typeof(float).IsAssignableFrom(type))
			{
				var view = new FloatField();
				view.label = "_";
				view.RegisterValueChangedCallback(evt => { viewValue.SetValue(evt.newValue); });
				view.value = (float)viewValue.GetValue();
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
				view.value = (double)viewValue.GetValue();

				control = view;
				return true;
			}

			if (typeof(Vector4).IsAssignableFrom(type))
			{
				var view = new Vector4Field();
				view.label = "_";
				view.RegisterValueChangedCallback(evt => { viewValue.SetValue(evt.newValue); });
				view.value = (Vector4)viewValue.GetValue();

				control = view;
				return true;
			}

			if (typeof(Vector3).IsAssignableFrom(type))
			{
				var view = new Vector3Field();
				view.label = "_";
				view.RegisterValueChangedCallback(evt => { viewValue.SetValue(evt.newValue); });
				view.value = (Vector3)viewValue.GetValue();

				control = view;
				return true;
			}

			if (typeof(Vector3Int).IsAssignableFrom(type))
			{
				var view = new Vector3IntField();
				view.label = "_";
				view.RegisterValueChangedCallback(evt => { viewValue.SetValue(evt.newValue); });
				view.value = (Vector3Int)viewValue.GetValue();

				control = view;
				return true;
			}

			if (typeof(Vector2).IsAssignableFrom(type))
			{
				var view = new Vector2Field();
				view.label = "_";
				view.RegisterValueChangedCallback(evt => { viewValue.SetValue(evt.newValue); });
				view.value = (Vector2)viewValue.GetValue();

				control = view;
				return true;
			}

			if (typeof(Vector2Int).IsAssignableFrom(type))
			{
				var view = new Vector2IntField();
				view.label = "_";
				view.RegisterValueChangedCallback(evt => { viewValue.SetValue(evt.newValue); });
				view.value = (Vector2Int)viewValue.GetValue();

				control = view;
				return true;
			}

			if (typeof(Color).IsAssignableFrom(type))
			{
				var view = new ColorField();
				view.label = "_";
				view.RegisterValueChangedCallback(evt => { viewValue.SetValue(evt.newValue); });
				view.value = (Color)viewValue.GetValue();

				control = view;
				return true;
			}


			control = null;
			return false;
		}
	}
}
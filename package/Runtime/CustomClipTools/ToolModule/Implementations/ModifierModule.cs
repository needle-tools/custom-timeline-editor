#nullable enable
using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Needle.Timeline.CustomClipTools.ToolModule.Implementations
{
	public class ModifierModule : CoreToolModule, IWeighted
	{
		public ModifierModule() => AllowBinding = true;

		[Range(.1f, 10)] public float Radius = 1;
		[Range(0, 1)] public float Probability = 1;
		[PowerSlider(0, 1, 3)] public float Weight = 1;

		float IWeighted.Weight
		{
			get => Weight;
			set => Weight = value;
		}

		public override bool CanModify()
		{
			return IsAnyBindingEnabled();
		}

		// public override bool CanModify(Type type) => true;

		// TODO: how can we make sure we know what to return here
		protected override IList<Type> SupportedTypes { get; } =
			new[] { typeof(Enum), typeof(Vector3), typeof(float), typeof(double), typeof(Color), typeof(int) };

		private static ProfilerMarker modifierModuleMarker = new ProfilerMarker("ModifierModule.OnModify");

		protected override ToolInputResult OnModifyValue(InputData input, ref ModifyContext context, ref object value)
		{
			using (modifierModuleMarker.Auto())
			{
				if (Random.value > Probability) return ToolInputResult.Failed;
				var pos = ToolHelpers.TryGetPosition(context.Object, value);
				if (pos == null) return ToolInputResult.Failed;

				float? weight = null;
				if (context.Object is IWeightProvider<InputData> prov)
				{
					weight = prov.GetCustomWeight(this, input);
					// if returned weight is not null assume it is valid if above 0
					if (weight != null && weight.Value < 0) return ToolInputResult.Failed;
				}

				if (weight == null)
				{
					var screenDistance = input.GetRadiusDistanceScreenSpace(Radius, pos.Value);
					if (screenDistance == null || screenDistance > 1) return ToolInputResult.Failed;
					weight = 1 - screenDistance.Value;
				}
				if (weight == null) return ToolInputResult.Failed;
				context.Weight = weight.Value * Weight;
				return ToolInputResult.Success;
			}
		}
	}


	// public class PaintColor : CoreToolModule
	// {
	// 	[Range(.1f, 10)]
	// 	public float Radius = 1;
	// 	[Range(0.01f, 3)] public float Falloff = 1;
	//
	// 	protected override IList<Type> SupportedTypes { get; } = new[] { typeof(Color) };
	//
	// 	protected override bool AllowedButton(MouseButton button)
	// 	{
	// 		return button == MouseButton.LeftMouse || button == MouseButton.RightMouse;
	// 	}
	//
	// 	protected override bool AllowedModifiers(InputData data, EventModifiers current)
	// 	{
	// 		return current == EventModifiers.None;
	// 	}
	//
	// 	protected override ToolInputResult OnModifyValue(InputData input, ref ModifyContext context, ref object value)
	// 	{
	// 		if (value is Color col)
	// 		{
	// 			float strength = 1;
	// 			var pos = ToolHelpers.TryGetPosition(context.Object, value);
	// 			if (pos == null) return ToolInputResult.Failed;
	// 			
	// 			var sp = input.ToScreenPoint(pos.Value);
	//
	// 			var dist = Vector2.Distance(input.StartScreenPosition, sp) / 100;
	// 			strength = Mathf.Clamp01(((Radius - dist) / Radius) / Falloff);
	// 			if (strength <= 0.001f) return ToolInputResult.Failed;
	//
	// 			// TODO: we need to have access to other fields of custom types, e.g. here we want the position to get the distance
	//
	// 			// TODO: figure out how we create new objects e.g. in a list
	//
	// 			Color.RGBToHSV(col, out var h, out var s, out var v);
	// 			h += input.ScreenDelta.x * .005f;
	// 			if ((Event.current.button == (int)MouseButton.RightMouse))
	// 				s += input.ScreenDelta.y * .01f;
	// 			else
	// 				v += input.ScreenDelta.y * .01f;
	// 			col = Color.HSVToRGB(h, s, v);
	// 			value = Color.Lerp((Color)value, col, strength);
	// 		}
	// 		return ToolInputResult.Success;
	// 	}
	// }

	// public class FloatScaleDrag : ToolModule
	// {
	// 	public float Radius = 1;
	//
	// 	public override bool CanModify(Type type)
	// 	{
	// 		return typeof(float).IsAssignableFrom(type);
	// 	}
	//
	// 	public override bool OnModify(InputData input, ref ToolData toolData)
	// 	{
	// 		if (toolData.Value is float vec)
	// 		{
	// 			if (toolData.Position != null)
	// 			{
	// 				var dist = Vector2.Distance(input.StartScreenPosition, input.ToScreenPoint(toolData.Position.Value));
	// 				var strength = Mathf.Clamp01(Radius * 100 - dist);
	// 				if (strength <= 0) return false;
	// 				var delta = input.ScreenDelta.y * 0.01f;
	// 				var target = vec + delta;
	// 				toolData.Value = target;
	// 				return true;
	// 			}
	// 			else
	// 			{
	// 				var delta = -input.ScreenDelta.y * 0.01f;
	// 				var target = vec + delta;
	// 				toolData.Value = target;
	// 				return Mathf.Abs(delta) > .0001f;
	// 			}
	// 		}
	// 		return false;
	// 	}
	// }


	// public class SetFloatValue : ToolModule
	// {
	// 	public float Value = .1f;
	//
	// 	public override bool CanModify(Type type)
	// 	{
	// 		return typeof(float).IsAssignableFrom(type);
	// 	}
	//
	// 	public override bool OnModify(InputData input, ref ToolData toolData)
	// 	{
	// 		if (toolData.Value is float)
	// 		{
	// 			if (toolData.Position != null && input.WorldPosition != null)
	// 			{
	// 				var dist = Vector3.Distance(input.WorldPosition.Value, toolData.Position.Value);
	// 				var strength = Mathf.Clamp01(1 - dist);
	// 				if (strength <= 0) return false;
	// 				toolData.Value = Value;
	// 				return true;
	// 			}
	// 			toolData.Value = Value;
	// 			return true;
	// 		}

	// 		return false;

	// 	}
	// }
}
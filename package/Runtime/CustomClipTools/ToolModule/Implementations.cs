#nullable enable
using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Needle.Timeline
{
	public class SprayProducer : CoreToolModule
	{
		public SprayProducer() => AllowBinding = true; 
		
		public float Radius = 1;
		public int Max = 1000;
		[Range(0, 1)] public float Offset = 1;
		public bool OnSurface = false;
		public bool AllKeyframes = false;

		protected override IList<Type> SupportedTypes { get; } = new[] { typeof(Vector3), typeof(Vector2) };

		protected override IEnumerable<ICustomKeyframe?> GetKeyframes(ToolData toolData)
		{
			foreach (var kf in base.GetKeyframes(toolData))
			{
				if (IsCloseKeyframe(toolData, kf))
					yield return kf;
				else 
					yield return CreateAndAddNewKeyframe(toolData);
			}

			if (AllKeyframes)
			{
				foreach (var kf in toolData.Clip.Keyframes)
				{
					var keyframe = kf as ICustomKeyframe;
					if (IsCloseKeyframe(toolData, keyframe)) continue;
					yield return keyframe;
				}
			} 
		}

		protected override IEnumerable<ProducedValue> OnProduceValues(InputData input, ProduceContext context)
		{
			if (input.HasKeyPressed(KeyCode.M)) yield break;
			if (context.Count >= Max) yield break;
			if (input.WorldPosition == null) yield break; 

			var offset = Random.insideUnitSphere * Radius;
			var pos = input.WorldPosition.Value + offset;

			if (OnSurface && input.WorldNormal != null)
			{
				var screenPoint = input.ScreenPosition + Random.insideUnitCircle * input.GetRadiusInPixel(Radius).Value;
				var ray = input.ToRay(screenPoint);
				if (Physics.Raycast(ray.origin, ray.direction, out var hit, Radius * 100))
				{
					Debug.DrawLine(hit.point,hit.point + hit.normal, Color.green, 1);
					pos = hit.point; 
				}
				else yield return new ProducedValue(pos, false);
			}
			pos += Offset * input.WorldNormal.GetValueOrDefault() * Radius;

			if (input.IsIn2DMode) pos.z = 0;
			yield return new ProducedValue(pos, true);
		}
	}


	public class Spreader : CoreToolModule
	{
		protected override IList<Type> SupportedTypes { get; } = new []{typeof(Vector3)};

		[Range(.1f, 10)]
		public float Radius = 1;
		[Range(-1,1)]
		public float Strength = .5f;
		
		protected override ToolInputResult OnModifyValue(InputData input, ref ModifyContext context, ref object value)
		{
			var vec = (Vector3)value.Cast(typeof(Vector3));
			var dist = input.GetRadiusDistanceScreenSpace(Radius, vec);
			if (dist < 1)
			{
				return ToolInputResult.CaptureForFinalize;
			}
			return ToolInputResult.Failed;
		}

		protected override ToolInputResult OnModifyCaptured(InputData input, List<CapturedModifyContext> captured)
		{
			var sum = new Vector3();
			for (var index = 0; index < captured.Count; index++)
			{
				var e = captured[index];
				var pos = (Vector3)e.Value.Cast(typeof(Vector3));
				sum += pos;
			}
			var center = sum / captured.Count;
			var factor = 0.01f * Strength;
			for (var index = 0; index < captured.Count; index++)
			{
				var e = captured[index];
				var pos = (Vector3)e.Value.Cast(typeof(Vector3));
				var dir = center - pos;
				dir.Normalize();
				dir *= factor;
				e.Value = pos - dir;
				captured[index] = e;
			}

			return ToolInputResult.Success;
		}
	}

	public class DragPosition : CoreToolModule
	{
		[Range(0.01f, 10)]
		public float Radius = 1;
		[Range(0,1)]
		public float Probability = 1f;
		[Range(0,1)]
		public float Strength = 1f;

		protected override IList<Type> SupportedTypes { get; } = new[] { typeof(Vector3), typeof(Vector2) };

		protected override ToolInputResult OnModifyValue(InputData input, ref ModifyContext context, ref object value)
		{
			if (input.WorldPosition == null) return ToolInputResult.Failed;
			if (Random.value > Probability) return ToolInputResult.Failed;
			
			var vec = (Vector3)value.Cast(typeof(Vector3));
			
			var dist = input.GetRadiusDistanceScreenSpace(Radius, vec);
			if (dist < 1)
			{
				vec += input.DeltaWorld.Value * Strength;
				value = vec;
				return ToolInputResult.Success;
			}
			return ToolInputResult.Failed;
		}
	}

	public class Eraser : CoreToolModule
	{
		public float Radius = 1;

		protected override IList<Type> SupportedTypes { get; } = new[] { typeof(Vector3), typeof(Vector2) };

		protected override ToolInputResult OnDeleteValue(InputData input, ref DeleteContext context)
		{
			if (input.WorldPosition == null) return ToolInputResult.Failed;
			var vec = (Vector3)context.Value.Cast(typeof(Vector3));
			var dist = input.GetRadiusDistanceScreenSpace(Radius, vec);
			if (dist <= 1)
			{
				context.Deleted = true;
				return ToolInputResult.Success;
			}
			return ToolInputResult.Failed;
		}
	}

	public class ModifierModule : CoreToolModule, IWeighted
	{
		public ModifierModule() => AllowBinding = true; 
		
		[Range(.1f, 3)]
		public float Radius = 1;
		[Range(0,1)]
		public float Probability = 1;
		[Range(0,1)]
		public float Weight = 1;
		
		float IWeighted.Weight
		{
			get => Weight;
			set => Weight = value;
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
				
				if(weight == null)
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
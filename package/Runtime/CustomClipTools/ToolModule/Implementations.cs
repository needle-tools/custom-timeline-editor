#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace Needle.Timeline
{
	public class SprayProducer : CoreToolModule
	{
		[Range(0, 1)] public float Probability = 1;
		public float Radius = 1;
		public int Max = 1000;
		[Range(0, 1)] public float Offset = 1;
		public bool OnSurface = false;
		public bool AllKeyframes = false;

		protected override IEnumerable<Type> SupportedTypes { get; } = new[] { typeof(Vector3), typeof(Vector2) };

		protected override IEnumerable<ICustomKeyframe?> GetKeyframes(ToolData toolData)
		{
			foreach (var kf in base.GetKeyframes(toolData))
				yield return kf;

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

			var offset = Random.insideUnitSphere * Radius;
			var pos = input.WorldPosition.Value + offset;

			if (OnSurface && input.WorldNormal != null)
			{
				if (Physics.SphereCast(pos, Radius * .5f, -input.WorldNormal.Value, out var hit, Radius * 2f))
					pos = hit.point;
				else
				{
					yield return new ProducedValue(pos, true);
				}
			}
			pos += Offset * input.WorldNormal.GetValueOrDefault() * Radius;

			if (input.IsIn2DMode) pos.z = 0;
			yield return new ProducedValue(pos, true);
		}
	}

	public class DragPosition : CoreToolModule
	{
		public float Radius = 1;

		protected override IEnumerable<Type> SupportedTypes { get; } = new[] { typeof(Vector3), typeof(Vector2) };

		protected override bool OnModifyValue(InputData input, ModifyContext context, ref object value)
		{
			var vec = (Vector3)value.Cast(typeof(Vector3));
			if (Vector3.Distance(vec, input.WorldPosition.Value) < Radius)
			{
				vec += input.DeltaWorld.Value;
				value = vec;
				return true;
			}
			return true;
		}
	}

	public class Eraser : CoreToolModule
	{
		public float Radius = 1;

		protected override IEnumerable<ICustomKeyframe?> GetKeyframes(ToolData toolData)
		{
			yield return toolData.Clip.GetClosest(toolData.Time);
		}

		protected override IEnumerable<Type> SupportedTypes { get; } = new[] { typeof(Vector3), typeof(Vector2) };

		protected override bool OnDeleteValue(InputData input, ref DeleteContext context)
		{
			if (input.WorldPosition == null) return false;
			var vec = (Vector3)context.Value.Cast(typeof(Vector3));
			if (Vector3.Distance(vec, input.WorldPosition.Value) < Radius)
			{
				context.Deleted = true;
			}
			return true;
		}
	}

	public class IntModule : ToolModule
	{
		public float Radius = 1;

		public override bool CanModify(Type type)
		{
			return typeof(int).IsAssignableFrom(type) || typeof(Enum).IsAssignableFrom(type);
		}

		public override bool OnModify(InputData input, ref ToolData toolData)
		{
			foreach (var e in dynamicFields)
			{
				var dist = Vector3.Distance(input.WorldPosition.GetValueOrDefault(), (Vector3)toolData.Position.GetValueOrDefault());
				if (dist < Radius)
				{
					// Debug.Log("PAINT WITH " + e.GetValue() + " on " + toolData.ValueType);
					toolData.Value = e.GetValue();
					return true;
				}
			}
			return base.OnModify(input, ref toolData);
		}
	}


	public class FloatScaleDrag : ToolModule
	{
		public float Radius = 1;

		public override bool CanModify(Type type)
		{
			return typeof(float).IsAssignableFrom(type);
		}

		public override bool OnModify(InputData input, ref ToolData toolData)
		{
			if (toolData.Value is float vec)
			{
				if (toolData.Position != null)
				{
					var dist = Vector2.Distance(input.StartScreenPosition, input.ToScreenPoint(toolData.Position.Value));
					var strength = Mathf.Clamp01(Radius * 100 - dist);
					if (strength <= 0) return false;
					var delta = input.ScreenDelta.y * 0.01f;
					var target = vec + delta;
					toolData.Value = target;
					return true;
				}
				else
				{
					var delta = -input.ScreenDelta.y * 0.01f;
					var target = vec + delta;
					toolData.Value = target;
					return Mathf.Abs(delta) > .0001f;
				}
			}
			return false;
		}
	}


	public class SetFloatValue : ToolModule
	{
		public float Value = .1f;

		public override bool CanModify(Type type)
		{
			return typeof(float).IsAssignableFrom(type);
		}

		public override bool OnModify(InputData input, ref ToolData toolData)
		{
			if (toolData.Value is float)
			{
				if (toolData.Position != null && input.WorldPosition != null)
				{
					var dist = Vector3.Distance(input.WorldPosition.Value, toolData.Position.Value);
					var strength = Mathf.Clamp01(1 - dist);
					if (strength <= 0) return false;
					toolData.Value = Value;
					return true;
				}
				toolData.Value = Value;
				return true;
			}
			return false;
		}
	}

	public class DragColor : ToolModule
	{
		public float Radius = 1;
		[UnityEngine.Range(0.01f, 3)] public float Falloff = 1;

		public override bool CanModify(Type type)
		{
			return typeof(Color).IsAssignableFrom(type);
		}

		protected override bool AllowedButton(MouseButton button)
		{
			return button == MouseButton.LeftMouse || button == MouseButton.RightMouse;
		}

		protected override bool AllowedModifiers(InputData data, EventModifiers current)
		{
			return current == EventModifiers.None;
		}

		public override bool OnModify(InputData input, ref ToolData toolData)
		{
			if (toolData.Value is Color col)
			{
				float strength = 1;
				if (toolData.Position != null && input.StartWorldPosition != null)
				{
					var dist = Vector3.Distance(input.StartWorldPosition.Value, toolData.Position.Value);
					strength = Mathf.Clamp01(((Radius - dist) / Radius) / Falloff);
					if (strength <= 0.001f) return false;
				}

				// TODO: we need to have access to other fields of custom types, e.g. here we want the position to get the distance

				// TODO: figure out how we create new objects e.g. in a list

				Color.RGBToHSV(col, out var h, out var s, out var v);
				h += input.ScreenDelta.x * .005f;
				if ((Event.current.button == (int)MouseButton.RightMouse))
					s += input.ScreenDelta.y * .01f;
				else
					v += input.ScreenDelta.y * .01f;
				col = Color.HSVToRGB(h, s, v);
				toolData.Value = Color.Lerp((Color)toolData.Value, col, strength);
				return true;
			}
			return false;
		}
	}
}
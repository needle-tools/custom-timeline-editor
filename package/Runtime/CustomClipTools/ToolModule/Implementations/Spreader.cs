using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace Needle.Timeline.CustomClipTools.ToolModule.Implementations
{
	public class Spreader : CoreToolModule
	{
		protected override IList<Type> SupportedTypes { get; } = new[] { typeof(Vector3) };

		public SpreadMode Mode = SpreadMode.Neighbor;

		[Range(.1f, 10)] public float Radius = 1;
		[Range(-10, 10)] public float Strength = 1f;
		[Range(0, 1)] public float Falloff;

		public enum SpreadMode
		{
			Center = 0,
			Neighbor = 1,
		}

		private struct SpreadData
		{
			public float RadiusDist;
		}

		protected override bool AllowedButton(MouseButton button)
		{
			return base.AllowedButton(button) || button == MouseButton.RightMouse;
		}

		protected override ToolInputResult OnModifyValue(InputData input, ref ModifyContext context, ref object value)
		{
			var vec = (Vector3)value.Cast(typeof(Vector3));
			var dist = input.GetRadiusDistanceScreenSpace(Radius, vec);
			if (dist < 1)
			{
				context.AdditionalData = new SpreadData() { RadiusDist = dist.Value };
				return ToolInputResult.CaptureForFinalize;
			}
			return ToolInputResult.Failed;
		}


		protected override ToolInputResult OnModifyCaptured(InputData input, List<CapturedModifyContext> captured)
		{
			var factor = 0.01f;
			factor *= Strength;
			if (input.Button == MouseButton.RightMouse) factor *= -1;

			switch (Mode)
			{
				case SpreadMode.Center:
					OnSpreadFromCenter(input, captured, factor);
					break;
				case SpreadMode.Neighbor:
					OnSpreadFromNeighbors(input, captured, factor);
					break;
			}
			return ToolInputResult.Success;
		}

		private static List<Vector3> closestNeighbor = new List<Vector3>();

		private void OnSpreadFromNeighbors(InputData input, List<CapturedModifyContext> captured, float factor)
		{
			closestNeighbor.Clear();
			for (var index = 0; index < captured.Count; index++)
			{
				var e = captured[index];
				var pos = (Vector3)e.Value.Cast(typeof(Vector3));
				var closest = default(Vector3);
				var closestDistance = float.MaxValue;
				for (var k = 0; k < captured.Count; k++)
				{
					if (k == index) continue;
					var other = captured[k];
					var otherPos = (Vector3)other.Value.Cast(typeof(Vector3));
					var dist = Vector3.Distance(pos, otherPos);
					if (dist < closestDistance)
					{
						closestDistance = dist;
						closest = otherPos;
					}
				}
				closestNeighbor.Add(closest);
			}

			for (var index = 0; index < captured.Count; index++)
			{
				var e = captured[index];
				var pos = (Vector3)e.Value.Cast(typeof(Vector3));
				var closest = closestNeighbor[index];
				var dir = closest - pos;
				var radiusDist01 = 1f;
				if (e.Context.AdditionalData is SpreadData data) radiusDist01 = data.RadiusDist;
				ModifyVector(ref dir, factor, radiusDist01);
				e.Value = pos - dir;
				captured[index] = e;
			}
			closestNeighbor.Clear();
		}

		private void OnSpreadFromCenter(InputData input, List<CapturedModifyContext> captured, float factor)
		{
			var sum = new Vector3();
			for (var index = 0; index < captured.Count; index++)
			{
				var e = captured[index];
				var pos = (Vector3)e.Value.Cast(typeof(Vector3));
				sum += pos;
			}
			var center = sum / captured.Count;
			for (var index = 0; index < captured.Count; index++)
			{
				var e = captured[index];
				var pos = (Vector3)e.Value.Cast(typeof(Vector3));
				var dir = center - pos;
				var radiusDist01 = 1f;
				if (e.Context.AdditionalData is SpreadData data) radiusDist01 = data.RadiusDist;
				ModifyVector(ref dir, factor, radiusDist01);
				e.Value = pos - dir;
				captured[index] = e;
			}
		}

		private void ModifyVector(ref Vector3 dir, float factor, float radiusDist01)
		{
			if (dir.ApproximatelyZeroLength()) dir = Random.insideUnitSphere;
			if (Falloff > 0)
			{
				dir *= Mathf.Clamp01((1 - radiusDist01) / Falloff);
			}
			dir *= factor;
			dir = Vector3.ClampMagnitude(dir, .1f);
		}
	}
}
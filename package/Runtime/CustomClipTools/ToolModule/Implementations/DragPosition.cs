using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Needle.Timeline.CustomClipTools.ToolModule.Implementations
{
	public class DragPosition : CoreToolModule
	{
		[PowerSlider(0.01f, 10, 2)] public float Radius = 1;
		[PowerSlider(0, 1, 1)] public float Probability = 1f;
		[Range(0, 2)] public float Strength = 1f;
		[Range(0, 1)] public float Falloff = 0;

		// TODO: we should hide falloff and such when capture is on
		public bool Capture = true;

		private readonly Dictionary<int, List<ModificationIdentifier>> captured = new Dictionary<int, List<ModificationIdentifier>>();

		protected override IList<Type> SupportedTypes { get; } = new[] { typeof(Vector3), typeof(Vector2) };

		public override bool OnModify(InputData input, ref ToolData toolData)
		{
			if (input.Stage == InputEventStage.End)
			{
				GetList(toolData.ClipHash).Clear();
			}
			return base.OnModify(input, ref toolData);
		}

		private List<ModificationIdentifier> GetList(int hash)
		{
			if (captured.TryGetValue(hash, out var list))
				return list;
			list = new List<ModificationIdentifier>();
			captured.Add(hash, list);
			return list;
		}

		protected override ToolInputResult OnModifyValue(InputData input, ref ModifyContext context, ref object value)
		{
			if (input.WorldPosition == null) return ToolInputResult.Failed;
			if (!Capture && Random.value > Probability) return ToolInputResult.Failed;
			if (!input.DeltaWorld.HasValue) return ToolInputResult.Failed;

			var vec = (Vector3)value.Cast(typeof(Vector3));

			var dist = input.GetRadiusDistanceScreenSpace(Radius, vec);
			if ((dist != null && dist <= 1) || Capture)
			{
				var delta = input.DeltaWorld.Value;

				if (Capture)
				{
					var list = GetList(context.ClipHash);
					ModificationIdentifier id;
					if (input.Stage == InputEventStage.Begin && dist <= 1 && Random.value <= Probability)
					{
						id = new ModificationIdentifier(context, CalculateFactor(dist.GetValueOrDefault()));
						list.Add(id);
					}
					else if (list.Contains(context.TargetHash, context.Index, context.MemberIndex, out id))
					{
						// it is in the list, we can proceed :)
						delta *= id.Weight;
					}
					else return ToolInputResult.Failed;
				}
				else
				{
					delta *= CalculateFactor(dist.GetValueOrDefault());
				}

				vec += delta;
				value = vec;
				return ToolInputResult.Success; 
			}


			return ToolInputResult.Failed;
		}

		private float CalculateFactor(float dist)
		{
			var factor = Strength;
			if (Falloff > 0)
			{
				factor = (1 - dist);
				factor /= Falloff;
				factor = Strength * Mathf.Clamp01(factor);
			}
			return factor;
		}
	}
}
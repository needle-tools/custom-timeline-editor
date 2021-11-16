using System;
using System.Collections.Generic;
using UnityEngine;

namespace Needle.Timeline.CustomClipTools.ToolModule
{
	public interface IHasDirection
	{
		Vector3 Start { get; set; }
		Vector3 End { get; set; }
	}
	
	public class AlignTool : CoreToolModule
	{
		public float Radius = 1;
		[Range(-2,2)]
		public float Strength = 1f;

		public bool Brush;

		protected override IList<Type> SupportedTypes { get; } = new[] { typeof(IHasDirection) };

		private struct DirectionData
		{
			public Vector3 Dir;
			public float Dist;
		}

		protected override ToolInputResult OnModifyValue(InputData input, ref ModifyContext context, ref object value)
		{
			if (!Brush && input.DeltaWorld == null) return ToolInputResult.AbortFurtherProcessing;
			
			if (value is IHasDirection align)
			{
				var dir = align.End - align.Start;
				var dist = input.GetRadiusDistanceScreenSpace(Radius, align.Start);
				if (dist < 1)
				{
					context.AdditionalData = new DirectionData() { Dir = dir, Dist = dist.Value };
					return ToolInputResult.CaptureForFinalize;
				}
			}
			return base.OnModifyValue(input, ref context, ref value);
		}

		protected override ToolInputResult OnModifyCaptured(InputData input, List<CapturedModifyContext> captured)
		{
			var averyDir = Vector3.zero;
			if (!Brush)
			{
				foreach (var entry in captured)
				{
					if (entry.Context.AdditionalData is DirectionData data)
					{
						averyDir += data.Dir;
					}
				}
				averyDir /= captured.Count;
				averyDir.Normalize();
			}
			else 
				averyDir = input.DeltaWorld!.Value.normalized * input.ScreenDelta.magnitude/5;

			foreach (var entry in captured)
			{
				if (entry.Value is IHasDirection dir && entry.Context.AdditionalData is DirectionData data)
				{
					var length = data.Dir.magnitude;
					if (Brush)
					{
						dir.End = Vector3.Lerp(dir.End, dir.Start + averyDir * length, (1-data.Dist) * Strength * .1f);
					}
					else
					{
						dir.End = Vector3.Lerp(dir.End, dir.Start + averyDir * length, (1-data.Dist) * Strength * .1f);
					}
				}
			}

			return ToolInputResult.Success;
		}
	}
}
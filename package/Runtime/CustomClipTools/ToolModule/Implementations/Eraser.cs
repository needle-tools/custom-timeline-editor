using System;
using System.Collections.Generic;
using UnityEngine;

namespace Needle.Timeline.CustomClipTools.ToolModule.Implementations
{
	public class Eraser : CoreToolModule
	{
		[PowerSlider(.1f, 10, 2)]
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
}
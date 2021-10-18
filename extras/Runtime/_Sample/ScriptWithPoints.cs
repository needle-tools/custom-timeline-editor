#nullable enable
using System.Collections.Generic;
using Needle.Timeline;
using UnityEditor;
using UnityEngine;

namespace _Sample._Sample
{
	public class ScriptWithPoints : MonoBehaviour, IAnimated, IOnionSkin
	{
		[Animate] public List<Point>? Points = new List<Point>();

		public struct Point : IToolEvents
		{
			public Vector3 Position;
			public float Weight;
			public Color Color;

			public void OnToolEvent(ToolStage stage, IToolData? _)
			{
				Weight = .05f;
				Color = Color.white;
			}
		}


		[Animate] public List<Direction>? Lines = new List<Direction>();

		[Animate] public List<Circle>? Circles = new List<Circle>();

		private void OnDrawGizmos()
		{
			RenderOnionSkin(OnionData.Default);
		}

		public void RenderOnionSkin(IOnionData data)
		{
			if (Points != null)
			{
				foreach (var pt in Points)
				{
					Gizmos.color = Color.Lerp(pt.Color, data.ColorOnion, data.WeightOnion);
					Gizmos.DrawSphere(pt.Position, pt.Weight + .01f);
				}
			}

			if (Lines != null)
			{
				Gizmos.color = Color.Lerp(Color.gray, data.ColorOnion, data.WeightOnion);
				foreach (var line in Lines)
				{
					line.DrawGizmos();
				}
			}

#if UNITY_EDITOR
			if (Circles != null)
			{
				for (var index = 0; index < Circles.Count; index++)
				{
					var circle = Circles[index];
					circle.RenderOnionSkin(data);
				}
			}
#endif
		}
	}
}
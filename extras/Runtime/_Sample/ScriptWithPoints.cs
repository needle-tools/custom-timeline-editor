using System;
using System.Collections.Generic;
using Needle.Timeline;
using UnityEngine;

namespace _Sample._Sample
{
	public class ScriptWithPoints : MonoBehaviour, IAnimated, IOnionSkin
	{
		[Animate] public List<Point> Points1 = new List<Point>();

		public struct Point : IInit
		{
			public Vector3 Position;
			public float Weight;
			public Color Color;

			public void Init(InitStage stage, IToolData _)
			{
				Weight = .05f;
				Color = Color.white;
			}
		}

		[Animate]
		public List<Line> Lines = new List<Line>();

		private void OnDrawGizmos()
		{
			RenderOnionSkin(0);
		}

		public void RenderOnionSkin(int layer)
		{
			var onionColor = new Color(1, 1, 1, .3f);
			var lerp = 0f;
			if (layer != 0)
			{
				lerp = 1f;
				if(layer < 0)
					onionColor = new Color(1f, .5f, .5f, .3f);
				else
					onionColor = new Color(0.5f, 1f, .5f, .3f);
			}
			
			if (Points1 != null)
			{
				foreach (var pt in Points1)
				{
					Gizmos.color = Color.Lerp(pt.Color, onionColor, lerp);
					Gizmos.DrawSphere(pt.Position, pt.Weight + .01f);
				}
			}

			if (Lines != null)
			{
				Gizmos.color = Color.Lerp(Color.white, onionColor, lerp);
				foreach (var line in Lines)
				{
					line.DrawGizmos();
				}
			}
		}
	}
}
using System;
using System.Collections.Generic;
using Needle.Timeline;
using UnityEngine;

namespace _Sample._Sample
{
	public class ScriptWithPoints : MonoBehaviour, IAnimated
	{
		[Animate] public List<Point> Points1 = new List<Point>();

		[Animate]
		public List<Line> Lines = new List<Line>();

		[Serializable]
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

		private void OnDrawGizmos()
		{
			if (Points1 != null)
			{
				foreach (var pt in Points1)
				{
					Gizmos.color = pt.Color;
					Gizmos.DrawSphere(pt.Position, pt.Weight + .01f);
				}
			}

			if (Lines != null)
			{
				Gizmos.color = Color.white;
				foreach (var line in Lines)
				{
					line.DrawGizmos();
				}
			}
		}
	}
}
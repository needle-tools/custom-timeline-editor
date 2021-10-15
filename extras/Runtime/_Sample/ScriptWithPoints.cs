using System;
using System.Collections.Generic;
using System.Threading;
using Needle.Timeline;
using UnityEngine;

namespace _Sample._Sample
{
	public class ScriptWithPoints : MonoBehaviour, IAnimated
	{
		
		[Animate]
		public List<Point> Points1 = new List<Point>();

		[Serializable]
		public struct Point
		{
			public Vector3 Position; 
			public float Weight;
		}

		private void OnDrawGizmos()
		{
			if (Points1 != null)
			{
				foreach (var pt in Points1)
				{
					Gizmos.DrawSphere(pt.Position, pt.Weight + .1f);
				}
			}
		}
	}
}
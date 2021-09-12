using System;
using System.Collections.Generic;
using DefaultNamespace;
using Needle.Timeline;
using UnityEngine;

namespace _Sample
{
	public class AnimatedScript : MonoBehaviour, IAnimated
	{
		[Animate]
		public float MyValue;
		
		[Animate]
		private float MyOthervalue;
		
		[NonSerialized]
		public List<Vector3> points = new List<Vector3>();

		private void OnDrawGizmosSelected()
		{
			var size = Vector3.up * .01f;
			Gizmos.color = Color.yellow;
			// Gizmos.DrawWireSphere();
			for (var index = 1; index < points.Count; index++)
			{
				var pt = points[index];
				Gizmos.DrawLine(pt, pt + size);
			}
		}
	}
}
using System.Collections.Generic;
using Needle.Timeline;
using UnityEngine;

namespace _Sample._Sample
{
	public class DummyScript : MonoBehaviour, IAnimated
	{
		[Animate]
		public List<Line> List;
		

		private void OnDrawGizmos()
		{
			if (List != null)
			{
				Gizmos.color = Color.cyan;
				foreach (var guide in List)
				{
					Debug.DrawLine(guide.Start, guide.End);
				}
			}
		}
	}
}
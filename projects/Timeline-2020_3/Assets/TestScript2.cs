using System.Collections.Generic;
using Needle.Timeline;
using UnityEngine;

namespace DefaultNamespace
{
	public class TestScript2 : MonoBehaviour, IAnimated
	{
		[Animate]
		private List<Vector3> OtherVecs;
		
		private void OnDrawGizmos()
		{
			Gizmos.color = Color.red;
			if(OtherVecs != null)
				foreach (var vec in OtherVecs)
					Gizmos.DrawSphere(vec, .1f);
		}
	}
}
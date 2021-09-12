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
	}
}
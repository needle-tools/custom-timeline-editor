using System;
using UnityEngine;

namespace Needle.Timeline.Samples
{
	[ExecuteAlways]
	public class ComputeBindingV2 : MonoBehaviour
	{
		private ComputeShaderRunner runner;

		private void OnEnable()
		{
			runner = new ComputeShaderRunner(this);
		} 
	}
}
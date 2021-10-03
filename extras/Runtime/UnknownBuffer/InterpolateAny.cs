using System;
using Codice.Client.BaseCommands;
using Needle.Timeline;
using UnityEngine;

namespace _Sample.UnknownBuffer
{
	[ExecuteInEditMode]
	public class InterpolateAny : MonoBehaviour
	{
		public ComputeShader Shader;
		
		private void OnEnable()
		{
			// TestInterpolateAny<float>(Shader, "FLOAT", sizeof(float), 0, 1);
			// TestInterpolateAny(Shader, "FLOAT2", sizeof(float)*2, new Vector2(), Vector2.one);
			// TestInterpolateAny(Shader, "FLOAT3", sizeof(float)*3, new Vector3(), Vector3.one);
			// TestInterpolateAny(Shader, "FLOAT2", sizeof(float)*2, new MyType(), new MyType(){v0=1, v1=1});
		}
	}
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Needle.Timeline.Samples
{
	/// <summary>
	/// This simple script just shows how C# is automatically bound to an assigned shader and dispatched
	/// </summary>
	[ExecuteAlways]
	public class SimpleComputeSample : Animated
	{
		// the shader is automatically detected by the base class
		public ComputeShader MyShader;
		
		// note that the following fields match the compute shader fields
		[TextureInfo(256, 256)] 
		public RenderTexture Result;
		public Color MyColor;


		protected override void OnEnable() => OnRequestEvaluation();
		private void OnValidate() => OnRequestEvaluation();

		
		
		public Renderer Output;
		
		protected override void OnAfterEvaluation()
		{
			base.OnAfterEvaluation();
			Output.sharedMaterial.mainTexture = Result;
		}
	}
}
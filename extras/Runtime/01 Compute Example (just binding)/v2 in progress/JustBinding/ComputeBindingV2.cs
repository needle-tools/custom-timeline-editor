using System;
using System.Collections.Generic;
using UnityEngine;

namespace Needle.Timeline.Samples
{
	[ExecuteAlways]
	public class ComputeBindingV2 : MonoBehaviour
	{
		public ComputeShader Shader;
		
		[TextureInfo(512, 512)] 
		public RenderTexture Result;
		public Color MyColor;

		public Renderer Renderer;

		private void Update()
		{
			Shader.Run(this, "DrawBackground", 0, 0, 0);
			if (Renderer?.sharedMaterial)
				Renderer.sharedMaterial.mainTexture = Result;
		}
	}
}
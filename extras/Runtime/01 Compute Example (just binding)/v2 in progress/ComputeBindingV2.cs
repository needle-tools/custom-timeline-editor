using System;
using System.Collections.Generic;
using UnityEngine;

namespace Needle.Timeline.Samples
{
	[ExecuteAlways]
	public class ComputeBindingV2 : MonoBehaviour
	{
		private ComputeShaderRunner runner;
		
		public ComputeShader Shader;
		[TextureInfo(512, 512)] 
		public RenderTexture Result;
		public Color MyColor;

		private void OnEnable() => CreateRunner();
		private void OnValidate() => CreateRunner();

		private void CreateRunner()
		{
			runner?.Dispose();
			runner = new ComputeShaderRunner(this, Shader);
		}

		private void Update()
		{
			runner.DispatchAll();
			ShowTexture();
		}
		
		

		[Header("Visualization")] public Renderer Renderer;

		private void ShowTexture()
		{
			if (Renderer?.sharedMaterial)
				Renderer.sharedMaterial.mainTexture = Result;
		}
	}
}
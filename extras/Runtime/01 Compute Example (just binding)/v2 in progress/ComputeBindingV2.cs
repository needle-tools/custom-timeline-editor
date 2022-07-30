using System;
using UnityEngine;

namespace Needle.Timeline.Samples
{
	[ExecuteAlways]
	public class ComputeBindingV2 : MonoBehaviour
	{
		public ComputeShader Shader;
		[TextureInfo(512, 512)] public RenderTexture Result;

		public Color MyColor;

		[Header("Visualization")] public Renderer Renderer;

		private ComputeShaderRunner runner;

		private void OnEnable()
		{
			runner?.Dispose();
			runner = new ComputeShaderRunner(this, Shader);
		}

		private void OnValidate()
		{
			runner?.Dispose();
			runner = new ComputeShaderRunner(this, Shader);
		}

		private void Update()
		{
			runner.DispatchAll();
			if (Renderer?.sharedMaterial)
				Renderer.sharedMaterial.mainTexture = Result;
		}
	}
}
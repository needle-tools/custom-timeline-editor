using System;
using System.Collections.Generic;
using UnityEngine;

namespace Needle.Timeline.Samples
{
	[ExecuteAlways]
	public class ComputeBindingV2 : ComputeShaderRunnerUnityComponent
	{
		[TextureInfo(512, 512)] public RenderTexture Result;
		public Color MyColor;

		[Header("Visualization")] public Renderer Renderer;

		protected override void Update()
		{
			base.Update();
			if (Renderer?.sharedMaterial)
				Renderer.sharedMaterial.mainTexture = Result;
		}
	}
}
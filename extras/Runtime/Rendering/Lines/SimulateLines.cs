using System;
using _Sample._Sample;
using Needle.Timeline;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace _Sample.Rendering.Lines
{
	[ExecuteAlways]
	public class SimulateLines : MonoBehaviour, IAnimated
	{
		public int Width = 1024, Height = 720;
		public float WidthWorld = 1;

		public AnimatedPoints Points;
		public ComputeShader Shader;
		public Renderer Renderer;

		public float FadeInSpeed = 10;
		
		private RenderTexture _texture;

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.gray;
			Gizmos.DrawWireCube(Vector3.zero, Size);
		}

		private Vector3 Size
		{
			get
			{
				var aspect = Width / (float)Height;
				return new Vector3(WidthWorld, WidthWorld / aspect, 0);
			}
		}

		private void Update()
		{
			_texture.SafeCreate(ref _texture, Width, Height, 0, GraphicsFormat.R32G32B32A32_SFloat, true);
			
			if (Renderer)
			{
				Renderer.transform.position = Vector3.zero;
				Renderer.transform.localScale = Size;
				Renderer.sharedMaterial.mainTexture = _texture;
			}

			if (Shader)
			{
				Shader.SetTime();
				
				Shader.SetTexture(0, "Output", _texture);
				Shader.SetBuffer(0, "Positions", Points.points, sizeof(float) * 3);
				Shader.SetFloat("XFactor", Width/WidthWorld);
				Shader.SetFloat("FadeInSpeed", FadeInSpeed);
				Shader.DispatchOptimal(0,  Points.points.Count, 1, 1);
				
				// clear
				Shader.SetTexture(1, "Output", _texture);
				Shader.DispatchOptimal(1,  Width, Height, 1);
			}
		}
	}
}
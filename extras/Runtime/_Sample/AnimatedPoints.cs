using System;
using System.Collections.Generic;
using Needle.Timeline;
using UnityEngine;
using UnityEngine.UI;

namespace _Sample._Sample
{
	[ExecuteAlways]
	public class AnimatedPoints : MonoBehaviour, IAnimated
	{
		[Animate, NonSerialized]
		public List<Vector3> points = new List<Vector3>(); 
		
		[Animate, NonSerialized]
		public List<Vector3> list = new List<Vector3>();

		[Animate]
		public float gizmoSizeFactor = 1;

		public int pointsCount;

		private ComputeBuffer buffer;
		public ComputeShader Shader;

		public RawImage Output;

		private RenderTexture texture;


		private void Update()
		{
			pointsCount = points?.Count ?? 0;
			if (pointsCount <= 0) return;
			if (buffer == null || !buffer.IsValid() || buffer.count < points.Count)
			{
				if(buffer?.IsValid() ?? false) buffer.Release();
				buffer = new ComputeBuffer(points.Count, sizeof(float) * 3, ComputeBufferType.Structured);
			}

			if (!texture)
			{
				if(texture) texture.Release();
				texture = new RenderTexture(32, 32, 0);
				texture.enableRandomWrite = true;
				texture.Create();
			}
			Graphics.Blit(Texture2D.blackTexture, texture);

			Output.texture = texture;

			buffer.SetData(points);
			Shader.SetBuffer(0, "_Points", buffer);
			Shader.SetTexture(0, "_Result", texture);
			Shader.Dispatch(0, Mathf.CeilToInt(points.Count / 32f), 1, 1);
		}
		
		private void OnDrawGizmos()
		{
			var size = Vector3.up * .01f;
			Gizmos.color = Color.yellow;
			if (points == null) return;
			for (var index = 0; index < points.Count; index++) 
			{ 
				var pt = points[index];
				Gizmos.DrawLine(pt, pt + size);
				Gizmos.DrawSphere(pt, .1f * gizmoSizeFactor);
			}
		}
	}
}
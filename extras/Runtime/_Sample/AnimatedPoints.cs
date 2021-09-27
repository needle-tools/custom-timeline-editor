using System;
using System.Collections.Generic;
using Needle.Timeline;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace _Sample._Sample
{
	public struct Guide
	{
		public Vector3 Start;
		public Vector3 End;
	}

#if UNITY_EDITOR
	public class GuideTool : CustomClipToolBase
	{
		private Vector3? lastPt = null;
		
		public override bool Supports(Type type)
		{
			return typeof(List<Guide>).IsAssignableFrom(type);
		}

		protected override void OnToolInput()
		{
			if (Event.current.type == EventType.MouseDown && Event.current.modifiers == EventModifiers.None)
			{
				if (lastPt != null)
				{
					var clip = ActiveClip;
					var kf = clip.GetClosest((float)CurrentTime);
					var guide = new Guide() { Start = lastPt.GetValueOrDefault(), End = GetCurrentMousePositionInScene() };
					if (kf != null && Mathf.Abs((float)CurrentTime - kf.time) < .1f)
					{
						var guides = kf.value as List<Guide>;
						guides.Add(guide);
						kf.RaiseValueChangedEvent();
					}
					else
					{
						clip.Add(new CustomKeyframe<List<Guide>>(new List<Guide>() { guide }, (float)CurrentTime));
					}
					lastPt = null;
				}
				else
				{
					lastPt = GetCurrentMousePositionInScene();
				}
			}
		}
	}
#endif

	[ExecuteAlways]
	public class AnimatedPoints : MonoBehaviour, IAnimated
	{
		[Animate, NonSerialized] public List<Vector3> points = new List<Vector3>();

		[Animate, NonSerialized] public List<Guide> guides = new List<Guide>();

		[Animate] public float gizmoSizeFactor = 1;

		public int pointsCount;

		private ComputeBuffer buffer;
		public ComputeShader Shader;

		public RawImage Output;

		private RenderTexture texture;


		private void Update()
		{
			pointsCount = points?.Count ?? 0;
			// if (pointsCount <= 0) return;
			// if (buffer == null || !buffer.IsValid() || buffer.count < points.Count)
			// {
			// 	if (buffer?.IsValid() ?? false) buffer.Release();
			// 	buffer = new ComputeBuffer(points.Count, sizeof(float) * 3, ComputeBufferType.Structured);
			// }
			//
			// if (!texture)
			// {
			// 	if (texture) texture.Release();
			// 	texture = new RenderTexture(32, 32, 0);
			// 	texture.enableRandomWrite = true;
			// 	texture.Create();
			// }
			// Graphics.Blit(Texture2D.blackTexture, texture);
			//
			// Output.texture = texture;
			//
			// buffer.SetData(points);
			// Shader.SetBuffer(0, "_Points", buffer);
			// Shader.SetTexture(0, "_Result", texture);
			// Shader.Dispatch(0, Mathf.CeilToInt(points.Count / 32f), 1, 1);
		}

		private void OnDrawGizmos()
		{
			var size = Vector3.up * .01f;
			if (points != null)
			{
				Gizmos.color = Color.yellow;
				for (var index = 0; index < points.Count; index++)
				{
					var pt = points[index];
					Gizmos.DrawLine(pt, pt + size);
					Gizmos.DrawSphere(pt, .1f * gizmoSizeFactor);
				}
			}

			if (guides != null)
			{
				Gizmos.color = Color.cyan;
				foreach (var guide in guides)
				{
					Debug.DrawLine(guide.Start, guide.End);
				}
			}
		}
	}
}
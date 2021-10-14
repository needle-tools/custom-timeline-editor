using System;
using System.Collections.Generic;
using _Sample._Sample;
using Needle.Timeline;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Random = UnityEngine.Random;

namespace _Sample.Rendering.Lines
{
	[ExecuteAlways]
	public class SimulateLines : MonoBehaviour, IAnimated, IAnimatedEvents
	{
		public int Width = 1024, Height = 720;
		public float WidthWorld = 1;

		public AnimatedPoints Points;
		public ComputeShader Shader;
		public Renderer Renderer;
		public bool UpdateViaTimeline = true;

		[Header("Settings")] public int Seed = 0;
		public int Count = 1000;
		public float FadeInSpeed = 10, FadeOutSpeed = 5;
		public float MoveSpeed = 10, TurnSpeed = 5;
		public float EnergyFactor = 1;

		private RenderTexture _texture;

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.gray;
			Gizmos.DrawWireCube(Vector3.zero, Size);
		}

		private void OnValidate()
		{
			this.RequestBuffer();
		}

		private Vector3 Size
		{
			get
			{
				var aspect = Width / (float)Height;
				return new Vector3(WidthWorld, WidthWorld / aspect, 0);
			}
		}

		private struct Entity
		{
			public Vector2 pos;
			public Vector2 lastPos;
			public Vector2 dir;
			public float energy;
		}

		private readonly List<Entity> entities = new List<Entity>();
		private ComputeBuffer entitiesBuffer;
		private int maxBufferSize = 0;
		private int currentBufferSize;

		private void OnEnable()
		{
			entities.Clear();
			entitiesBuffer.SafeDispose();
			maxBufferSize = 0;
			if (_texture) Graphics.Blit(Texture2D.blackTexture, _texture);
		}

		private void Update()
		{
			if (!UpdateViaTimeline)
				Run();
		}

		public void OnReset()
		{
			entities.Clear();
			entitiesBuffer.SafeDispose();
			maxBufferSize = 0;
			currentBufferSize = 0;
			if (_texture) Graphics.Blit(Texture2D.blackTexture, _texture);
		}

		public void OnEvaluated(FrameInfo frame)
		{
			if (UpdateViaTimeline)
				Run();
		}

		private void Run()
		{
			if (!Points) return;
			if (Renderer.IsHidden())
			{
				return;
			}
			// Debug.Log("RUN");
			// Debug.Log("RUN");
			_texture.SafeCreate(ref _texture, Width, Height, 0, GraphicsFormat.R32G32B32A32_SFloat, true);

			if (Renderer)
			{
				Renderer.transform.position = Vector3.zero;
				Renderer.transform.localScale = Size;
				Renderer.sharedMaterial.mainTexture = _texture;
			}
			
			if (Shader)
			{
				this.SetTime(Shader);

				if (entities.Count <= 0 || entities.Count != Count)
				{
					entities.Clear();
					Random.InitState(Seed);
					for (var i = 0; i < Count; i++)
					{
						var e = new Entity() { pos = 0.2f * Random.insideUnitCircle * new Vector2(Width, Height) * Random.value };
						e.lastPos = e.pos;
						e.dir = Vector2.up;
						e.energy = 1;
						entities.Add(e);
					}
					var stride = typeof(Entity).GetStride();
					entitiesBuffer = ComputeBufferProvider.GetBuffer("Entities", entities, stride);
				}
				Shader.SetBuffer("Simulate", "Entities", entitiesBuffer); 
				if (MoveSpeed != 0)   
					Shader.SetFloat("MoveSpeed", MoveSpeed);
				if (TurnSpeed != 0)
					Shader.SetFloat("TurnSpeed", TurnSpeed);
				var requiredSize = Mathf.Max(Points.points?.Count ?? 0, 100);
				if (maxBufferSize < requiredSize) maxBufferSize = Mathf.CeilToInt(requiredSize * 1.5f);
				// Debug.Log((Points.points?.Count ?? 0) + " => "+  maxBufferSize);
				if (Points.points != null)    
					Shader.SetBuffer("Simulate", "Positions", Points.points, sizeof(float) * 3, maxBufferSize);
				Shader.SetInt("PositionsCount", Points.points?.Count ?? 0);
				Shader.SetFloat("EnergyFactor", EnergyFactor);
				Shader.DispatchOptimal("Simulate", entities.Count, 1, 1);

				Shader.SetTexture("Draw", "Output", _texture);
				Shader.SetBuffer("Draw", "Entities", entitiesBuffer);
				Shader.SetFloat("XFactor", Width / WidthWorld);
				Shader.SetFloat("FadeInSpeed", FadeInSpeed);
				Shader.DispatchOptimal("Draw", entities.Count, 1, 1);

				// clear
				Shader.SetTexture("Clear", "Output", _texture);
				Shader.SetFloat("FadeOutSpeed", FadeOutSpeed);
				Shader.DispatchOptimal("Clear", Width, Height, 1);
			}
		}
	}
}
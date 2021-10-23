#nullable enable

using System;
using System.Collections.Generic;
using Needle.Timeline;
using UnityEngine;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
public class SimulateCircleColor : Animated, IOnionSkin
{
	public ComputeShader Shader;

	[Animate] public List<Circle> Circles;
	[Animate, ShaderField("Dots")] public List<ColorDot> ColorDots; 

	private const int entitiesCount = 20000;  
	
	[Manual]
	public ComputeBuffer? Entities; 

	public struct Entity
	{
		public Vector3 Position;
		public float Energy;
	}
	
	[TextureInfo(1024, 1024, TextureFormat = TextureFormat.RGBA32)]
	public RenderTexture DataTexture; 

	[TextureInfo(1024, 1024, TextureFormat = TextureFormat.RGBA32)]
	public RenderTexture Result; 

	public Vector2 WorldScale;

	[Header("Debug")] public Renderer Output, Data;

	public struct ColorDot : IOnionSkin, IToolEvents
	{
		public Vector3 Position; 
		public Color Color;
		public float Weight;

		public void RenderOnionSkin(IOnionData data)
		{
			Gizmos.color = data.GetColor(Color);
			Gizmos.DrawWireSphere(Position, Weight * .1f);
		}

		public void OnToolEvent(ToolStage stage, IToolData? data)
		{
			if (stage == ToolStage.BasicValuesSet)
			{
				Color = Color.white;
				Weight = 1f; 
			}
		}
	}

	public override void OnReset()
	{
		base.OnReset();
		Entities?.Dispose();
		Entities = null;
		if (Result) Graphics.Blit(Texture2D.blackTexture, Result);
		if (DataTexture) Graphics.Blit(Texture2D.blackTexture, DataTexture);
	}

	protected override IEnumerable<DispatchInfo> OnDispatch()
	{
		if (Output)
		{
			var lossyScale = Output.transform.lossyScale;
			WorldScale = new Vector2(lossyScale.x, lossyScale.y); 
		}
		if (Entities?.IsValid() == false || Entities?.count != entitiesCount)
		{
			Entities = Resources.ComputeBufferProvider.GetBuffer(nameof(Entities), entitiesCount, typeof(Entity).GetStride(), ComputeBufferType.Structured);
			var entities = new Entity[entitiesCount];
			for (var i = 0; i < Entities.count; i++)
			{
				entities[i] = new Entity()
				{
					Position = Random.insideUnitCircle*5
				};
			}
			Entities.SetData(entities); 
		}
		yield return new DispatchInfo() { KernelIndex = 0, GroupsX = entitiesCount }; 
		yield return new DispatchInfo() { KernelName = "CSRenderEntities", GroupsX = entitiesCount };
	}

	protected override void OnAfterEvaluation()
	{
		base.OnAfterEvaluation();

		if (Output && Result)
		{
			outputBlock ??= new MaterialPropertyBlock();
			outputBlock.SetTexture("_MainTex", Result);
			Output.SetPropertyBlock(outputBlock);
		}

		if (Data && DataTexture)
		{
			dataBlock ??= new MaterialPropertyBlock();
			dataBlock.SetTexture("_MainTex", DataTexture);
			Data.SetPropertyBlock(dataBlock);
		}
	}

	private MaterialPropertyBlock? outputBlock, dataBlock;

	private void OnDrawGizmos()
	{
		RenderOnionSkin(OnionData.Default);
	}

	public void RenderOnionSkin(IOnionData data)
	{
		if (Circles != null)
		{
			Gizmos.color = Color.Lerp(Color.gray, data.ColorOnion, data.WeightOnion);
			foreach (var c in Circles)
			{
				c.RenderOnionSkin(data);
			}
		}
		if (ColorDots != null)
		{
			Gizmos.color = Color.Lerp(Color.gray, data.ColorOnion, data.WeightOnion);
			foreach (var c in ColorDots)
			{
				c.RenderOnionSkin(data);
			}
		}
	}
}
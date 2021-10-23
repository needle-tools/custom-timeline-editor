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
	[Animate] public List<Direction> Directions;

	public int EntitiesCount = 20000;  
	
	[Manual]
	public ComputeBuffer? Entities; 

	public struct Entity
	{
		public Vector3 Position;
		public Vector2 StartPosition;
		public float Energy;
	}
	
	[TextureInfo(128, 128, TextureFormat = TextureFormat.RGBA32)]
	public RenderTexture DataTexture; 
	
	[TextureInfo(1024, 1024, TextureFormat = TextureFormat.RGBA32)]
	public RenderTexture DataTextureBig; 

	[TextureInfo(2048, 2048, TextureFormat = TextureFormat.RGBA32)]
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

	protected override void OnBeforeDispatching()
	{
		if (Output) 
		{
			var lossyScale = Output.transform.lossyScale;
			WorldScale = new Vector2(lossyScale.x, lossyScale.y); 
		}
		if (Entities?.IsValid() == false || Entities?.count != EntitiesCount)
		{
			Entities = Resources.ComputeBufferProvider.GetBuffer(nameof(Entities), EntitiesCount, typeof(Entity).GetStride(), ComputeBufferType.Structured);
			var entities = new Entity[EntitiesCount];
			for (var i = 0; i < EntitiesCount; i++)
			{
				var ent = new Entity()
				{
					Position = Random.insideUnitCircle * WorldScale.x * .5f
				};
				ent.StartPosition = ent.Position;
				entities[i] = ent;
			}
			Entities.SetData(entities);  
		}
	}

	protected override IEnumerable<DispatchInfo> OnDispatch()
	{
		yield return new DispatchInfo { KernelIndex = 0, GroupsX = EntitiesCount }; 
		if (Result) Graphics.Blit(Texture2D.blackTexture, Result);
		// yield return new DispatchInfo { KernelName = "CSRenderEntities", GroupsX = EntitiesCount };
		yield return new DispatchInfo { KernelName = "CSDistanceField", GroupsX = DataTexture.width, GroupsY = DataTexture.height}; 
		Graphics.Blit(DataTexture, DataTextureBig);
		yield return new DispatchInfo { KernelName = "CSRenderData", GroupsX = Result.width, GroupsY = Result.height}; 
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

		if (Data && DataTextureBig)
		{
			dataBlock ??= new MaterialPropertyBlock();
			dataBlock.SetTexture("_MainTex", DataTextureBig);
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
		if (Directions != null)
		{
			Gizmos.color = Color.Lerp(Color.gray, data.ColorOnion, data.WeightOnion);
			foreach (var c in Directions)
			{
				c.RenderOnionSkin(data);
			}
		}
	}
}
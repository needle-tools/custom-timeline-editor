#nullable enable

using System;
using System.Collections.Generic;
using Needle.Timeline;
using UnityEngine;

[ExecuteInEditMode]
public class SimulateCircleColor : Animated, IOnionSkin
{
	public ComputeShader Shader;

	[Animate] public List<Circle> Circles;
	[Animate, ShaderField("Dots")] public List<ColorDot> ColorDots;

	[ComputeBufferInfo(1,sizeof(float), Mode = ComputeBufferMode.Circular)]
	public ComputeBuffer Append;

	[TextureInfo(1024, 1024, TextureFormat = TextureFormat.RGBA32)]
	public RenderTexture Result; 

	public Vector2 WorldScale;

	[Header("Debug")] public Renderer Output;

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
		if (Result) Graphics.Blit(Texture2D.blackTexture, Result);
	}

	protected override IEnumerable<DispatchInfo> OnDispatch()
	{
		if (Output)
		{
			var lossyScale = Output.transform.lossyScale;
			WorldScale = new Vector2(lossyScale.x, lossyScale.y);
		}
		yield return new DispatchInfo() { KernelIndex = 0, GroupsX = 1024, GroupsY = 1024 };
	}

	protected override void OnAfterEvaluation()
	{
		base.OnAfterEvaluation();
		block ??= new MaterialPropertyBlock();
		block.SetTexture("_MainTex", Result);
		Output.SetPropertyBlock(block);
	}

	private MaterialPropertyBlock block;

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
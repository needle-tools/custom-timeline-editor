using System;
using System.Collections.Generic;
using Needle.Timeline;
using UnityEngine;

[ExecuteInEditMode]
public class SimulateCircleColor : Animated, IOnionSkin, IAnimatedEvents
{
	public ComputeShader Shader;

	[Animate] public List<Circle> Circles;

	[TextureInfo(1024, 1024, TextureFormat = TextureFormat.RGBA32)]
	public RenderTexture Result;


	public float TimeFactor = 1;
	public float OffsetY;
	[Range(0,1), ShaderField("MyField")]
	public float XOffset;
	
	
	[Header("Debug")] 
	public Renderer Output;
	
	
	
	private void OnDrawGizmos()
	{
		RenderOnionSkin(OnionData.Default);
	}

	public void RenderOnionSkin(IOnionData data)
	{
		if (Circles != null)
		{
			foreach (var c in Circles)
			{
				c.RenderOnionSkin(data);
			}
		}
	}

	protected override void OnEndOfDispatch()
	{
		base.OnEndOfDispatch();
		block ??= new MaterialPropertyBlock();
		block.SetTexture("_MainTex", Result);
		Output.SetPropertyBlock(block);
	}

	private MaterialPropertyBlock block;
}
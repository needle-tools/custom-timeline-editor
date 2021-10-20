using System;
using System.Collections.Generic;
using Needle.Timeline;
using UnityEngine;

[ExecuteInEditMode]
public class SimulateCircleColor : Animated, IOnionSkin
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
	

	protected override IEnumerable<DispatchInfo> OnDispatch()
	{
		yield return new DispatchInfo() { KernelIndex = 0, GroupsX = 1024, GroupsY = 1024 };
	}
	
	protected override void OnUpdated()
	{
		base.OnUpdated();
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
			foreach (var c in Circles)
			{
				c.RenderOnionSkin(data);
			}
		}
	}
}
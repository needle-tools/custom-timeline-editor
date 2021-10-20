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

	public Vector2 WorldScale;
	
	[Header("Debug")] 
	public Renderer Output; 

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
			foreach (var c in Circles)
			{
				c.RenderOnionSkin(data);
			}
		}
	}
}
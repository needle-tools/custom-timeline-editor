using System;
using System.Collections.Generic;
using Needle.Timeline;
using UnityEngine;

public class DrawLine : Animated
{
	public ComputeShader Shader;
	[Animate] public List<Direction> Directions;
	public Transform Start;
	public Transform End;
	[TextureInfo(256,256)]
	public RenderTexture Output; 
	public Renderer Rend;  


	protected override IEnumerable<DispatchInfo> OnDispatch()
	{
		yield return new DispatchInfo { KernelIndex = 0, GroupsX = 1 };
		yield return new DispatchInfo { KernelIndex = 1, GroupsX = 256, GroupsY = 256};
	}

	protected override void OnAfterEvaluation()
	{
		base.OnAfterEvaluation(); 
		Rend.SetTexture(Output);
	}

	private void OnDrawGizmos()
	{
		if(Directions == null) return;
		foreach(var dir in Directions) dir.RenderOnionSkin(OnionData.Default);
	}
}
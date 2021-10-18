using System;
using System.Collections.Generic;
using Needle.Timeline;
using UnityEngine;

[ExecuteInEditMode]
public class SimulateCircleColor : MonoBehaviour, IAnimated, IOnionSkin
{
	public ComputeShader Shader;

	[Animate]
	public List<Circle> Circles;

	[SerializeField]
	private  List<ComputeShaderFieldInfo> infos = new List<ComputeShaderFieldInfo>();

	// private void OnValidate()
	// {
	// 	if(enabled)
	// 		Shader.TryParse(infos);
	// }
	//
	// private void OnEnable()
	// {
	// 	Shader.TryParse(infos);
	// }

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
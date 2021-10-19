using System;
using System.Collections.Generic;
using Needle.Timeline;
using UnityEngine;

[ExecuteInEditMode]
public class SimulateCircleColor : MonoBehaviour, IAnimated, IOnionSkin, IAnimatedEvents
{
	public ComputeShader Shader;

	[Animate] public List<Circle> Circles;

	[TextureInfo(11, 11, TextureFormat = TextureFormat.RGBA32)]
	public RenderTexture Result;

	public float OffsetY;
	
	
	
	[SerializeField, HideInInspector] 
	private ComputeShader lastShader;
	[SerializeField] private ComputeShaderInfo info;
	private List<ComputeShaderBinding> bindings = new List<ComputeShaderBinding>();
	private IResourceProvider resources = ResourceProvider.CreateDefault();

	[Header("Debug")] 
	public Renderer Output;

	private void OnValidate()
	{
		OnUpdate();
	}

	private void OnEnable()
	{
		Bind();
	}

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

	private void Bind()
	{
		if (lastShader == Shader && info == null) return;
		lastShader = Shader;
		Shader.TryParse(out info);
	}

	public void OnReset()
	{
	}

	public void OnEvaluated(FrameInfo frame)
	{
		OnUpdate();
	}

	private void OnUpdate()
	{
		Bind();
		if (bindings == null) return;
		
		bindings.Clear();
		this.SetTime(Shader);
		info.Bind(GetType(), bindings, resources);
		info.Dispatch(this, 0, bindings, new Vector3Int(Result.width, Result.height, 1));

		block ??= new MaterialPropertyBlock();
		block.SetTexture("_MainTex", Result);
		Output.SetPropertyBlock(block);
	}

	private MaterialPropertyBlock block;
}
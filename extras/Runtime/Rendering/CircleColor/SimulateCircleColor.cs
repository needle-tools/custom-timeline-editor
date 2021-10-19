using System;
using System.Collections.Generic;
using Needle.Timeline;
using UnityEngine;

[ExecuteInEditMode]
public class SimulateCircleColor : MonoBehaviour, IAnimated, IOnionSkin, IAnimatedEvents
{
	public ComputeShader Shader;

	[Animate] public List<Circle> Circles;

	[TextureInfo(1024, 1024, TextureFormat = TextureFormat.RGBA32)]
	public RenderTexture Result;


	public float TimeFactor = 1;
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
		info = null;
		EnsureShaderParsed();
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

	private void EnsureShaderParsed()
	{
		if (lastShader == Shader && info == null) return;
		lastShader = Shader;
		Shader.TryParse(out info);
	}

	public void OnReset()
	{
		info = null;
	}

	public void OnEvaluated(FrameInfo frame)
	{
		OnUpdate();
	}

	private void OnUpdate()
	{
		EnsureShaderParsed();
		if (info == null) return;
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
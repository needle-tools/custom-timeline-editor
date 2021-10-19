using System;
using System.Collections.Generic;
using Needle.Timeline;
using UnityEngine;

[ExecuteInEditMode]
public class SimulateCircleColor : MonoBehaviour, IAnimated, IOnionSkin, IAnimatedEvents
{
	public ComputeShader Shader;
	[SerializeField, HideInInspector] private ComputeShader lastShader;

	[Animate] public List<Circle> Circles;

	[SerializeField] private ComputeShaderInfo info;
	private List<ComputeShaderBinding> bindings = new List<ComputeShaderBinding>();
	private IResourceProvider resources = ResourceProvider.CreateDefault();

	private void OnValidate()
	{
		if (lastShader == Shader) return;
		lastShader = Shader;
		Shader.TryParse(out info);
	}

	private void OnEnable()
	{
		Shader.TryParse(out info);
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

	public void OnReset()
	{
	}

	public void OnEvaluated(FrameInfo frame)
	{
		bindings.Clear();
		info.Bind(GetType(), bindings, resources);
		info.Dispatch(this, 0, bindings);
	}
}
using System.Collections.Generic;
using Needle.Timeline;
using UnityEngine;
using Random = UnityEngine.Random;

public class DrawLine : Animated
{ 
	public ComputeShader Shader;
	[Animate] public List<Direction> Directions; 
	public Transform Start;
	public Transform End;
	[TextureInfo(256,256, FilterMode = FilterMode.Point)]
	public RenderTexture Output; 
	public Renderer Rend;

	// TODO: how can we specify WHEN a field should be set, for example: i only want to initialize points and then mark dirty or something to notify that the buffer should be reset
	private List<Point> Points;
	public struct Point 
	{
		public Vector2 Pos;
	}

	public override void OnReset()
	{
		base.OnReset();
		Points?.Clear();
	}

	protected override void OnBeforeDispatching()
	{
		Graphics.Blit(Texture2D.blackTexture, Output);
	}

	protected override IEnumerable<DispatchInfo> OnDispatch()
	{
		// yield return new DispatchInfo { KernelIndex = 1, GroupsX = 32, GroupsY = 32};
		// yield return new DispatchInfo { KernelIndex = 1, GroupsX = Directions?.Count };

		if (Points == null || Points.Count <= 0)
		{
			Points ??= new List<Point>();
			for (var i = 0; i < 12; i++)
			{
				Points.Add(new Point(){Pos = Random.insideUnitCircle*.3f});
			}
		}
		
		yield return new DispatchInfo { KernelIndex = 2, GroupsX = Points?.Count }; 
		
		
		yield return new DispatchInfo { KernelIndex = 0, GroupsX = 1 };
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
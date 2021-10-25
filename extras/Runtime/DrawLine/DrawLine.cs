using System.Collections.Generic;
using System.Linq;
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

	[TransformInfo]
	public List<Transform> TransformList = new List<Transform>();
	[TransformInfo(Data = TransformInfo.DataType.Rotation)]
	public Transform[] TransformArray;

	private List<Point> Points;
	public struct Point 
	{
		public Vector2 Pos;
	}

	public int Points_Count = 100;
	public float PointSpacing = .2f;

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
		yield return new DispatchInfo { KernelIndex = 1, GroupsX = Directions?.Count };

		if (Points == null || Points.Count <= 0 || Points.Count != Points_Count)
		{
			Points ??= new List<Point>();
			Points.Clear();
			for (var i = 0; i < Points_Count; i++)
			{
				Points.Add(new Point(){Pos = Random.insideUnitCircle*.05f});
			}
			Debug.Log("Points: " + Points.Count());
			SetDirty(nameof(Points));
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
using System;
using System.Collections.Generic;
using System.Linq;
using Needle.Timeline;
using Needle.TransformExtensions;
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

	private Color Color;

	[TransformInfo]
	public List<Transform> TransformList = new List<Transform>();
	public Transform[] TransformArray;

	private List<Point> Points;
	public struct Point 
	{
		public Vector2 Pos;
	}

	public int Points_Count = 100;
	public float PointSpacing = .2f;

	private void OnValidate()
	{
		foreach(var t in TransformArray)
			t.OnHasChanged(OnRequestEvaluation);
	}

	[ContextMenu("Reset now")]
	public override void OnReset()
	{
		base.OnReset();
		Points?.Clear(); 
	}

	protected override void OnBeforeDispatching()
	{
		// Graphics.Blit(Texture2D.blackTexture, Output); 
	}

	protected override IEnumerable<DispatchInfo> OnDispatch()
	{
		if (transform.childCount != TransformArray?.Length)
		{
			// TODO: use PlayerLoopHelper to create transform changed watcher that resets changed bool at very end of every frame
			
			TransformArray = new Transform[transform.childCount];
			for(var i = 0; i < transform.childCount; i++)
			{
				var t = transform.GetChild(i);
				TransformArray[i] = t;
				t.OnHasChanged(OnRequestEvaluation);
			}
		}
		
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
		if (PointSpacing < .00001f)
		{
			Points_Count = (int)Random.Range(10, 200);
			Color = Random.ColorHSV(0,1,.3f,1,.5f,1);
		}
		yield return new DispatchInfo { KernelIndex = 2, GroupsX = Points?.Count }; 
		
		
		yield return new DispatchInfo { KernelIndex = 0, GroupsX = 1 };
		yield return new DispatchInfo { KernelName = "CSBlend" };
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
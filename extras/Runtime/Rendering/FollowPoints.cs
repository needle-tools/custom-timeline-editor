using System;
using _Sample._Sample;
using UnityEngine;

[ExecuteAlways]
public class FollowPoints : MonoBehaviour
{
	public int Count;
	public ComputeShader Shader;
	public AnimatedPoints Points;
	public Mesh Mesh;
	public Material InstancedMaterial;
	
	[Header("Settings")]
	public float Scale = 1;
	public float Speed = 1;
	public float Damping = 1;

	private ComputeBuffer input;
	private ComputeBuffer positions;
	private ComputeBuffer velocities;
	private ComputeBuffer args;
	private uint[] argsData;

	private void Update()
	{
		if (!Shader || !Points || Count <= 0) return;
		if (Points.pointsCount <= 0) return;
		if (!InstancedMaterial || !Mesh) return;
		if (!InstancedMaterial.enableInstancing)
		{
			Debug.LogWarning("Material does not support instancing", InstancedMaterial);
			return;
		}
		
		if (input == null || !input.IsValid() || input.count < Points.pointsCount)
		{
			input = new ComputeBuffer(Points.pointsCount, sizeof(float) * 3);
		}
		input.SetData(Points.points);

		if (positions == null || !positions.IsValid() || positions.count != Count)
		{
			positions = new ComputeBuffer(Count, sizeof(float) * 4);
		}

		if (velocities == null || velocities.count != positions.count)
		{
			velocities = new ComputeBuffer(Count, sizeof(float) * 3);
		}

		if (args == null)
		{
			args = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
			argsData = new uint[5];
		}
		argsData[0] = (uint)Mesh.GetIndexCount(0);
		argsData[1] = (uint)Count;
		argsData[2] = (uint)Mesh.GetIndexStart(0);
		argsData[3] = (uint)Mesh.GetBaseVertex(0);
		args.SetData(argsData);

		Shader.SetBuffer(0, "Input", input);  
		Shader.SetInt("CurrentCount", Points.pointsCount);
		Shader.SetBuffer(0, "Positions", positions);
		Shader.SetFloat("DeltaTime", Time.deltaTime);

		Shader.SetBuffer(0, "Velocities", velocities);
		Shader.SetFloat("ScaleFactor", Scale);
		Shader.SetFloat("SpeedFactor", Speed);
		Shader.SetFloat("DampFactor", Damping);
		
		var tx = Mathf.CeilToInt(positions.count / 32f);
		Shader.Dispatch(0, tx, 1, 1);

		InstancedMaterial.SetBuffer("Positions", positions);
		Graphics.DrawMeshInstancedIndirect(Mesh, 0, InstancedMaterial, new Bounds(Vector3.zero, 10000*Vector3.one), args);
	}
}

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Needle.Timeline
{
	public class ListInterpolator : IInterpolator<List<Vector3>>
	{
		// private ArrayInterpolator arrayInterpolator = new ArrayInterpolator();
		private List<Vector3> result;
		private List<Vector3> secondaryBuffer;

		public List<Vector3> Interpolate(List<Vector3> v0, List<Vector3> v1, float t)
		{
			// TODO: via timeline mixer it can currently happen that one of the inputs is the output list of a previous interpolation, maybe we need some buffer cache to get temporary result buffers?
			// if (v0 == result)
			// {
			// 	
			// }
			//
			// if (v1 == result)
			// {
			// 	
			// }
			
			if (result == null) result = new List<Vector3>();
			else result.Clear();
			var count = Mathf.RoundToInt(Mathf.Lerp(v0.Count, v1.Count, t));
			for (var i = 0; i < count; i++)
			{
				var val0 = v0.Count > 0 ? v0[i % v0.Count] : v1[i];
				var val1 = v1.Count > 0 ? v1[i % v1.Count] : v0[i];
				var res = Vector3.Lerp(val0, val1, t);
				result.Add(res);
			}

			//
			if (secondaryBuffer == null) secondaryBuffer = new List<Vector3>(result.Count);
			secondaryBuffer.Clear();
			secondaryBuffer.AddRange(result);
			return secondaryBuffer;
		}
	}

	public class ArrayInterpolator : IInterpolator<NativeArray<float3>>, IDisposable
	{
		public NativeArray<float3> TargetBuffer;

		public NativeArray<float3> Interpolate(NativeArray<float3> v0, NativeArray<float3> v1, float t)
		{
			var max = Mathf.Max(v0.Length, v1.Length);

			if (TargetBuffer.Length < max)
			{
				if (TargetBuffer.IsCreated) TargetBuffer.Dispose();
				TargetBuffer = new NativeArray<float3>(max, Allocator.Persistent);
			}

			var job = new InterpolatorJob<float3>();
			job.I0 = v0;
			job.I1 = v1;
			job.t = t;
			job.Target = TargetBuffer;
			var handle = job.Schedule(max, 32);
			handle.Complete();
			return TargetBuffer.GetSubArray(0, max);
		}

		public struct InterpolatorJob<T> : IJobParallelFor where T : struct
		{
			public NativeSlice<float3> I0;
			public NativeSlice<float3> I1;
			public float t;
			public NativeSlice<float3> Target;

			public void Execute(int index)
			{
				var v0 = I0[index % I0.Length];
				var v1 = I1[index % I1.Length];
				var res = math.lerp(v0, v1, t);
				Target[index] = res;
			}
		}

		public void Dispose()
		{
			if (TargetBuffer.IsCreated)
				TargetBuffer.Dispose();
		}
	}
}
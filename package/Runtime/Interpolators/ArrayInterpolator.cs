using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Needle.Timeline.Interfaces;
using UnityEditor;
using UnityEngine;

namespace Needle.Timeline
{

	public class IInterpolatableInterpolator : IInterpolator
	{
		public object Instance { get; set; }

		public bool CanInterpolate(Type type)
		{
			return typeof(IInterpolatable).IsAssignableFrom(type);
		}

		public object Interpolate(object v0, object v1, float t)
		{
			var i0 = v0 as IInterpolatable;
			var i1 = v1 as IInterpolatable;
			return null;
		}
	}

	[Priority(-100)]
	[NoAutoSelect]
	public class NumbersInterpolator : IInterpolator
	{
		private readonly Type[] types = new[]
		{
			typeof(int),
			typeof(uint),
			typeof(float),
			typeof(double)
		};

		public object Instance { get; set; }

		public bool CanInterpolate(Type type)
		{
			return types.Contains(type);
		}

		public object Interpolate(object v0, object v1, float t)
		{
			throw new NotImplementedException();
		}
	}

	[Priority(-100)]
	[NoAutoSelect]
	public class IndexingInterpolator : IInterpolator
	{
		private PropertyInfo indexer;
		private object[] args;

		public object Instance { get; set; }

		public bool CanInterpolate(Type type)
		{
			indexer = type.GetIndexer();
			if (indexer == null) return false;
			args ??= new object[1];
			return indexer != null;
		}

		public object Interpolate(object v0, object v1, float t)
		{
			if (t < 0.999f)
			{
				if (v0 == null) return null;
				return indexer.GetValue(v0);
			}
			if (v1 == null) return null;
			return indexer.GetValue(v1);
		}
	}

	public class ListInterpolator : IInterpolator<List<Vector3>>
	{
		private List<Vector3> result;
		private List<Vector3> secondaryBuffer;

		public object Instance { get; set; }

		public bool CanInterpolate(Type type)
		{
			return typeof(List<Vector3>).IsAssignableFrom(type);
		}

		public object Interpolate(object v0, object v1, float t)
		{
			return Interpolate(v0 as List<Vector3>, v1 as List<Vector3>, t);
		}

		public enum Mode
		{
			AllAtOnce = 0,
			Individual = 1
		}

		public Mode CurrentMode = Mode.Individual;

		public List<Vector3> Interpolate(List<Vector3> v0, List<Vector3> v1, float t)
		{
			// TODO: via timeline mixer it can currently happen that one of the inputs is the output list of a previous interpolation, maybe we need some buffer cache to get temporary result buffers?
			
			if (result == null) result = new List<Vector3>();
			else result.Clear();
			var count = Mathf.RoundToInt(Mathf.Lerp(v0?.Count ?? 0, v1?.Count ?? 0, t));
			var perEntry = 1f / count;
			for (var i = 0; i < count; i++)
			{
				var val0 = v0?.Count > 0 ? v0[i % v0.Count] : v1?[i];
				var val1 = v1?.Count > 0 ? v1[i % v1.Count] : v0?[i];
				if (val0 == null || val1 == null) continue;
				Vector3? res = null;
				switch (CurrentMode)
				{
					case Mode.AllAtOnce:
						res = Vector3.Lerp(val0.Value, val1.Value, t);
						break;

					case Mode.Individual:
						var start = (i) * perEntry;
						var it = Mathf.Clamp01(t - start) / perEntry;
						res = Vector3.Lerp(val0.Value, val1.Value, it);
						break;
				}
				if (res != null)
					result.Add(res.Value);
			}

			//
			if (secondaryBuffer == null) secondaryBuffer = new List<Vector3>(result.Count);
			secondaryBuffer.Clear();
			secondaryBuffer.AddRange(result);
			return secondaryBuffer;
		}
	}

	// public class ArrayInterpolator : IInterpolator<NativeArray<float3>>, IDisposable
	// {
	// 	public NativeArray<float3> TargetBuffer;
	//
	// 	public NativeArray<float3> Interpolate(NativeArray<float3> v0, NativeArray<float3> v1, float t)
	// 	{
	// 		var max = Mathf.Max(v0.Length, v1.Length);
	//
	// 		if (TargetBuffer.Length < max)
	// 		{
	// 			if (TargetBuffer.IsCreated) TargetBuffer.Dispose();
	// 			TargetBuffer = new NativeArray<float3>(max, Allocator.Persistent);
	// 		}
	//
	// 		var job = new InterpolatorJob<float3>();
	// 		job.I0 = v0;
	// 		job.I1 = v1;
	// 		job.t = t;
	// 		job.Target = TargetBuffer;
	// 		var handle = job.Schedule(max, 32);
	// 		handle.Complete();
	// 		return TargetBuffer.GetSubArray(0, max);
	// 	}
	//
	// 	public struct InterpolatorJob<T> : IJobParallelFor where T : struct
	// 	{
	// 		public NativeSlice<float3> I0;
	// 		public NativeSlice<float3> I1;
	// 		public float t;
	// 		public NativeSlice<float3> Target;
	//
	// 		public void Execute(int index)
	// 		{
	// 			var v0 = I0[index % I0.Length];
	// 			var v1 = I1[index % I1.Length];
	// 			var res = math.lerp(v0, v1, t);
	// 			Target[index] = res;
	// 		}
	// 	}
	//
	// 	public void Dispose()
	// 	{
	// 		if (TargetBuffer.IsCreated)
	// 			TargetBuffer.Dispose();
	// 	}
	// }
}
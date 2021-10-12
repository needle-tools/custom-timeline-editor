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

		private object _instance;

		public object Interpolate(object v0, object v1, float t)
		{
			if (v0 == null && v1 == null) return null;
			if (_instance == null)
			{
				var type = v0?.GetType() ?? v1.GetType();
				if (type == null) throw new Exception("Failed getting type?");
				_instance = Activator.CreateInstance(type);
			}
			if (v0 is IInterpolatable i0)
			{
				i0.Interpolate(ref _instance, v0, v1, t);
			}
			else if (v1 is IInterpolatable i1)
			{
				i1.Interpolate(ref _instance, v0, v1, t);
			}
			return _instance;
		}
	}

	[Priority(-100)]
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
			switch (v0, v1)
			{
				case (float f0, float f1): return Mathf.Lerp(f0, f1, t);
				case (double f0, double f1): return Lerp(f0, f1, t);
				case (int f0, int f1): return Mathf.Lerp(f0, f1, t);
				case (uint f0, uint f1): return Mathf.Lerp(f0, f1, t);
			}
			return v0;
		}

		private static double Lerp(double a, double b, float t)
		{
			return a + (b - a) * t;
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
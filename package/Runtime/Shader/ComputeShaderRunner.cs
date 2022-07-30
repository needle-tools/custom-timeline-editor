#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Needle.Timeline.ResourceProviders;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Needle.Timeline
{
	public readonly struct ComputeShaderDispatchJob
	{
		public int KernelIndex => kernel.Index;
		public string KernelName => kernel.Name;

		private readonly object target;
		private readonly ComputeShaderInfo shaderInfo;
		private readonly List<ComputeShaderBinding> bindings;
		private readonly ComputeShaderKernelInfo kernel;
		private readonly IDispatchCountProvider defaultCounts;

		public readonly bool DebugLog;

		public ComputeShaderDispatchJob(object target,
			ComputeShaderInfo shaderInfo,
			List<ComputeShaderBinding> bindings,
			ComputeShaderKernelInfo kernel,
			IDispatchCountProvider counts,
			bool debug = false
		)
		{
			this.target = target;
			this.shaderInfo = shaderInfo;
			this.bindings = bindings;
			this.kernel = kernel;
			defaultCounts = counts;
			DebugLog = debug;
		}
		public void Run<T1, T2, T3>(T1 x, T2 y, T3 z)
		{
			var counts = defaultCounts.GetCount();
			var kernelCounts = new KernelCounts<T1, T2, T3>(x, y, z).GetCount();
			if (kernelCounts.x >= 1) counts.x = kernelCounts.x;
			if (kernelCounts.y >= 1) counts.y = kernelCounts.y;
			if (kernelCounts.z >= 1) counts.z = kernelCounts.z;
			if (DebugLog)
				Debug.Log($"Run {kernel.Name} {counts}");
			shaderInfo.Dispatch(target, kernel.Index, bindings, counts);
		}
	}

	public struct KernelCounts<T1, T2, T3> : IDispatchCountProvider
	{
		public T1 X;
		public T2 Y;
		public T3 Z;

		public KernelCounts(T1 x, T2 y, T3 z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public Vector3Int GetCount()
		{
			var vec = Vector3Int.one;
			vec.x = ToInt(ref X, 0);
			vec.y = ToInt(ref Y, 1);
			vec.z = ToInt(ref Z, 2);
			return vec;
		}

		private static int ToInt<T>(ref T value, int componentIndex)
		{
			if (value == null) return -1;
			if (value is Object obj && obj == false) return -1;
			switch (value)
			{
				case int e: return e;
				case uint e: return (int)e;
				case float e: return (int)e;
				case double e: return (int)e;
				case long e: return (int)e;
				case IList e: return e.Count;
				case ComputeBuffer e: return e.count;
				case Texture tex:
					if (!tex) return -1;
					if (tex is Texture2D tex2)
					{
						switch (componentIndex)
						{
							case 0: return tex2.width;
							case 1: return tex2.height;
						}
					}
					if (tex is Texture3D tex3)
					{
						switch (componentIndex)
						{
							case 0: return tex3.width;
							case 1: return tex3.height;
							case 2: return tex3.depth;
						}
					}
					if (tex is RenderTexture rt)
					{
						switch (componentIndex)
						{
							case 0: return rt.width;
							case 1: return rt.height;
							case 2: return rt.depth;
						}
					}
					break;
			}
			return -1;
		}
	}

	public readonly struct ComputeShaderInfoBindingsGroup
	{
		public readonly ComputeShaderInfo Info;
		public readonly List<ComputeShaderBinding> Bindings;

		public ComputeShaderInfoBindingsGroup(ComputeShaderInfo info, List<ComputeShaderBinding> bindings)
		{
			Info = info;
			Bindings = bindings;
		}

		public bool IsValid => Info != null && Bindings != null;
	}

	public interface IDispatchCountProvider
	{
		Vector3Int GetCount();
	}

	public class DispatchDefaultCountProvider : IDispatchCountProvider
	{
		private readonly object target;
		private readonly List<ComputeShaderBinding> bindings = new List<ComputeShaderBinding>();

		internal void Add(ComputeShaderBinding binding)
		{
			this.bindings.Add(binding);
		}

		public DispatchDefaultCountProvider(object target)
		{
			this.target = target;
		}

		public Vector3Int GetCount()
		{
			var count = Vector3Int.one;
			// this list contains all the bindings that a kernel is writing to
			foreach (var bind in bindings)
			{
				var val = bind.GetValue(target);
				if (val == null) continue;
				var res = ComputeShaderUtils.TryGetKernelCountFor(val);
				if (res != null)
				{
					var kernelCount = res.Value;
					count.x = Mathf.Max(count.x, kernelCount.x);
					count.y = Mathf.Max(count.y, kernelCount.y);
					count.z = Mathf.Max(count.z, kernelCount.z);
				}
			}
			return count;
		}
	}

	public class ComputeShaderRunner : IDisposable
	{
		public object Target => target;
		public ComputeShader Shader => shader;
		public bool Debug = false;

		private readonly object target;
		private readonly Type targetType;
		private readonly ComputeShader shader;
		private ComputeShaderInfoBindingsGroup? group;
		private readonly IResourceProvider resources;
		private readonly List<IDispatchCountProvider> defaultDispatchCounts = new List<IDispatchCountProvider>();


		public ComputeShaderRunner(object target, ComputeShader shader, IResourceProvider? resourceProvider = default)
		{
			if (!shader) throw new ArgumentNullException(nameof(shader));
			this.target = target ?? throw new ArgumentNullException(nameof(target));
			this.shader = shader;
			this.targetType = target.GetType();
			this.resources = resourceProvider ?? ResourceProvider.CreateDefault();
		}

		public void Dispose()
		{
			shader.StopWatching();
			resources.Dispose();
		}
		
		/// <summary>
		/// XYZ are objects that can be used to get counts from for how many threads we want
		/// for example pass in Tex0, Tex0, List for a dispatch with x = Tex0.width, y = Tex0.height, z = List.Count
		/// </summary>
		public bool Run<T1, T2, T3>(string name, T1 x, T2 y, T3 z)
		{
			var res = Bind();
			if (!res.IsValid) return false;
			var info = res.Info;
			for (var i = 0; i < info.Kernels.Count; i++)
			{
				var kernel = info.Kernels[i];
				if (kernel.Name != name) continue;
				var defaultKernelCountProvider = defaultDispatchCounts[i];
				var job = new ComputeShaderDispatchJob(target, info, res.Bindings, kernel, defaultKernelCountProvider, Debug);
				job.Run(x, y, z);
				return true;
			}
			return false;
		}

		public void RunAll()
		{
			var res = Bind();
			if (!res.IsValid) return;
			var info = res.Info;
			for (var index = 0; index < info.Kernels.Count; index++)
			{
				var kernel = info.Kernels[index];
				var defaultKernelCountProvider = defaultDispatchCounts[index];
				info.Dispatch(target, kernel.Index, res.Bindings, defaultKernelCountProvider.GetCount());
			}
		}

		private ComputeShaderInfoBindingsGroup Bind()
		{
			if (!shader) return new ComputeShaderInfoBindingsGroup();
			// test if we have this bound already:
			// the shader was already parsed:
			if (group != null) return group.Value;
			// run again for it
			if (shader.TryParse(out var info) && info != null)
			{
				var bindings = new List<ComputeShaderBinding>();
				var newGroup = new ComputeShaderInfoBindingsGroup(info, bindings);
				group = newGroup;
				info.Bind(targetType, bindings, resources);
				shader.StartWatching(() => group = null);
				FindDefaultKernelCounts(target, info, bindings, defaultDispatchCounts);
				if (Debug)
					UnityEngine.Debug.Log("Did bind " + shader.name);
			}
			return group.GetValueOrDefault();
		}

		/// <summary>
		/// Try determine a good kernel count by looking at which fields a kernel writes to
		/// To later dispatch by the field having the biggest size (for each kernel)
		/// </summary>
		private static void FindDefaultKernelCounts(object instance,
			ComputeShaderInfo info,
			IList<ComputeShaderBinding> bindings,
			List<IDispatchCountProvider> list)
		{
			list.Clear();
			foreach (var kernel in info.Kernels)
			{
				var provider = new DispatchDefaultCountProvider(instance);
				list.Add(provider);
				foreach (var shaderBinding in bindings)
				{
					var shaderField = shaderBinding.ShaderField;
					if (shaderField.TryFindUsage(kernel, out var usage))
					{
						// we only want to know about writes to shader fields
						// assuming we use those for determining a good default dispatch count
						if ((usage & UsageType.Write) == 0) continue;
						var type = shaderField.FieldType;
						if (type != null)
						{
							var isRelevant = typeof(Texture).IsAssignableFrom(type) ||
							                 typeof(ComputeBuffer).IsAssignableFrom(type) ||
							                 typeof(IList).IsAssignableFrom(type)
								;

							if (isRelevant)
							{
								provider.Add(shaderBinding);
							}
						}
					}
				}
			}
		}
	}
}
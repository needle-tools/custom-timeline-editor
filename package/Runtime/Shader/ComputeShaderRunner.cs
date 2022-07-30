#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using Needle.Timeline.ResourceProviders;
using UnityEngine;

namespace Needle.Timeline
{
	// public readonly struct KernelIdentifier
	// {
	// 	public readonly int? Index;
	// 	public readonly string? Name;
	//
	// 	public KernelIdentifier(int? index, string? name)
	// 	{
	// 		Index = index;
	// 		Name = name;
	// 	}
	//
	// 	public bool IsValid => (Index != null && Index >= 0) || Name != null;
	// }
	//
	// public readonly struct DispatchInfo
	// {
	// 	public readonly KernelIdentifier Identifier;
	// 	public readonly Vector3Int? Counts;
	//
	// 	public DispatchInfo(KernelIdentifier identifier, Vector3Int counts)
	// 	{
	// 		Identifier = identifier;
	// 		Counts = counts;
	// 	}
	//
	// 	public DispatchInfo(KernelIdentifier identifier, object obj)
	// 	{
	// 		Identifier = identifier;
	// 		Counts = ComputeShaderUtils.TryGetKernelCountFor(obj) ?? Vector3Int.one;
	// 	}
	// }

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
			resources.Dispose();
		}

		public void DispatchAll()
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
			}
			return group.GetValueOrDefault();
		}

		/// <summary>
		/// Try determine a good kernel count by looking at which fields a kernel writes to
		/// To later dispatch by the field having the biggest size (for each kernel)
		/// </summary>
		private static void FindDefaultKernelCounts(object instance, ComputeShaderInfo info, IList<ComputeShaderBinding> bindings, List<IDispatchCountProvider> list)
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
							var isRelevant = typeof(Texture).IsAssignableFrom(type)  || 
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
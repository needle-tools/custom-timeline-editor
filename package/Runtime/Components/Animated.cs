#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Profiling;
using UnityEngine;

namespace Needle.Timeline
{
	[ExecuteInEditMode]
	public abstract class Animated : MonoBehaviour, IAnimated, IAnimatedEvents
	{
		// private void OnValidate()
		// {
		// 	InternalInit();
		// }

		private void OnEnable()
		{
			didSearchFields = false;
			InternalInit();
			ComputeShaderUtils.ComputeShaderChanged += OnShaderChanged;
		}

		private void OnDisable()
		{
			ComputeShaderUtils.ComputeShaderChanged -= OnShaderChanged;
		}

		private void OnShaderChanged(ComputeShader obj)
		{
			if (!obj) return;
			if (shaderInfos.Any(s => s != null && s.Shader == obj))
			{
				didSearchFields = false;
				
#pragma warning disable CS4014
				TimelineBuffer.RequestBufferCurrentInspectedTimeline();
#pragma warning restore CS4014
				// OnInternalEvaluate();
			}
		}

		private void InternalInit()
		{
			// didSearchFields = false;

			if (!didSearchFields)
			{
				didSearchFields = true;
				computeShaderFields.Clear();
				var type = GetType();
				var fields = type.GetFields(BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (var field in fields)
				{
					if (typeof(ComputeShader).IsAssignableFrom(field.FieldType))
					{
						computeShaderFields.Add(field);
					}
				}
				if (computeShaderFields.Count <= 0)
					Debug.LogWarning("No ComputeShader field found.", this);
				else
					Debug.Log("Found " + computeShaderFields.Count + " ComputeShader fields", this);
			}

			bindings.Clear();
			for (var index = 0; index < computeShaderFields.Count; index++)
			{
				var cs = computeShaderFields[index];
				ComputeShaderInfo? info;
				var shader = cs.GetValue(this) as ComputeShader;

				if (shader)
				{
					shader.StartWatching(() => didSearchFields = false);
					shader.TryParse(out info);
				}
				else info = null;

				info?.Bind(GetType(), bindings, resources);
				if (index >= shaderInfos.Count)
				{
					shaderInfos.Add(info);
				}
				else
				{
					shaderInfos[index] = info;
				}
			}

			evaluateMarker = new ProfilerMarker(GetType().Name + "." + nameof(OnInternalEvaluate));
		}

		protected IResourceProvider Resources => resources;

		private bool didSearchFields = false;
		private readonly List<FieldInfo> computeShaderFields = new List<FieldInfo>();
		private readonly List<ComputeShaderInfo?> shaderInfos = new List<ComputeShaderInfo?>();
		private readonly List<ComputeShaderBinding> bindings = new List<ComputeShaderBinding>();
		private readonly IResourceProvider resources = ResourceProvider.CreateDefault();
		private ProfilerMarker evaluateMarker;

		public virtual void OnReset()
		{
		}

		public void OnEvaluated(FrameInfo frame)
		{
			OnInternalEvaluate();
		}

		private void OnInternalEvaluate()
		{
			if (!didSearchFields) InternalInit();

			using (evaluateMarker.Auto())
			{
				var didDispatchAny = false;

				void BeforeDispatch()
				{
					if(!didDispatchAny)
						OnBeforeDispatching();
					didDispatchAny = true;
				}
				
				
				foreach (var dispatch in OnDispatch())
				{
					BeforeDispatch();
					DispatchNow(dispatch);
					OnDispatched(dispatch);
				}

				if (!didDispatchAny)
				{
					// dispatch default
					for (var index = 0; index < shaderInfos.Count; index++)
					{
						var shader = shaderInfos[index];
						if (shader == null) continue;
						this.SetTime(shader.Shader);
						// bindings.Clear();
						// shader.Bind(GetType(), bindings, resources);

						Vector3Int? kernelSize = null;
						foreach (var field in bindings)
						{
							if (!typeof(Texture).IsAssignableFrom(field.Field.FieldType)) continue;
							var info = field.Field.GetCustomAttribute<TextureInfo>();
							if (info == null) continue;
							if (info.Width > 0 && info.Height > 0)
							{
								kernelSize = new Vector3Int(info.Width, info.Height, info.Depth);
								break;
							}
						}
						
						foreach (var k in shader.Kernels)
						{
							BeforeDispatch();
							Debug.Log("Dispatch " + k.Name);
							shader.Dispatch(this, k.Index, bindings, kernelSize);
						}
					}
				}

				if (didDispatchAny)
				{
					OnAfterEvaluation();
				}
			}
		}
		
		protected virtual void OnBeforeDispatching()
		{
			
		}

		protected virtual void OnDispatched(DispatchInfo info)
		{
		}

		protected virtual void OnAfterEvaluation()
		{
		}

		private bool DispatchNow(DispatchInfo info)
		{
			if (!info.IsValid) return false;
			var foundKernel = false;
			for (var index = 0; index < shaderInfos.Count; index++)
			{
				var shader = shaderInfos[index];
				if (shader == null) continue;
				if (info.ShaderName != null && shader.Shader.name != info.ShaderName) continue;
				var kernel = shader.Kernels.FirstOrDefault(x =>
				{
					if (info.KernelIndex != null) return x.Index == info.KernelIndex;
					return x.Name == info.KernelName;
				});
				if (kernel == null) continue;
				foundKernel = true;
				this.SetTime(shader.Shader);
				// bindings.Clear();
				// shader.Bind(GetType(), bindings, resources);


				// if (AllowAutoThreadGroupSize() && info.GroupsX == null && info.GroupsY == null && info.GroupsZ == null)
				// {
				// 	foreach (var field in shader.Fields)
				// 	{
				// 		if (field.Kernels?.Any(x => x.Index == kernel.Index) ?? false)
				// 		{
				// 			if (typeof(Texture2D).IsAssignableFrom(field.FieldType))
				// 			{
				// 				
				// 			}
				// 		}
				// 	}
				// }

				shader.Dispatch(this, kernel.Index, bindings,
					new Vector3Int(info.GroupsX.GetValueOrDefault(), info.GroupsY.GetValueOrDefault(), info.GroupsZ.GetValueOrDefault()));
				foundKernel = true;
				break;
			}
			if (!foundKernel)
			{
				var msg = "Failed finding kernel ";
				if (info.ShaderName != null) msg += info.ShaderName + " : ";
				msg += info.KernelName ?? info.KernelIndex.ToString();
				Debug.LogWarning(msg, this);
			}
			return foundKernel;
		}

		public struct DispatchInfo
		{
			public string? ShaderName;
			public int? KernelIndex;
			public string? KernelName;
			public int? GroupsX;
			public int? GroupsY;
			public int? GroupsZ;

			public bool IsValid => KernelIndex != null || KernelName != null;
		}

		// protected virtual bool AllowAutoThreadGroupSize() => true;

		protected virtual IEnumerable<DispatchInfo> OnDispatch()
		{
			yield break;
		}
	}
}
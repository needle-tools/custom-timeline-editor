#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Needle.Timeline
{
	[ExecuteInEditMode]
	public abstract class Animated : MonoBehaviour, IAnimated, IAnimatedEvents
	{
		private void OnValidate()
		{
			InternalInit();
		}

		private void OnEnable()
		{
			didSearchFields = false;
			InternalInit();
		}

		private void InternalInit()
		{
			// didSearchFields = false;
			
			if (!didSearchFields)
			{ 
				didSearchFields = true;
				computeShaderFields.Clear();
				var type = GetType();
				var fields = type.GetFields( BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (var field in fields)
				{
					if (typeof(ComputeShader).IsAssignableFrom(field.FieldType))
					{
						computeShaderFields.Add(field);
					}
				}
				Debug.Log("Found " + computeShaderFields.Count + " compute shader fields");
			}

			var changed = true; 
			bindings.Clear();
			for (var index = 0; index < computeShaderFields.Count; index++)
			{
				var cs = computeShaderFields[index];
				ComputeShaderInfo? info;
				var shader = cs.GetValue(this) as ComputeShader;
				if (shader) shader.TryParse(out info);
				else info = null;

				if (index >= shaderInfos.Count)
				{
					shaderInfos.Add(info);
				}
				else
				{
					shaderInfos[index] = info;
				}
			}
		}

		private bool didSearchFields = false;
		private readonly List<FieldInfo> computeShaderFields = new List<FieldInfo>();
		private readonly List<ComputeShaderInfo?> shaderInfos = new List<ComputeShaderInfo?>();
		private readonly List<ComputeShaderBinding> bindings = new List<ComputeShaderBinding>();
		private readonly IResourceProvider resources = ResourceProvider.CreateDefault();

		public void OnReset()
		{
		}

		public void OnEvaluated(FrameInfo frame)
		{
			OnInternalEvaluate();
		}

		private void OnInternalEvaluate()
		{
			if (!didSearchFields) InternalInit();

			var didDispatchAny = false;
			foreach (var dispatch in OnDispatch())
			{
				didDispatchAny = true;
				DispatchNow(dispatch);
				OnDispatched(dispatch);
			}

			if (!didDispatchAny)
			{
				for (var index = 0; index < shaderInfos.Count; index++)
				{
					var shader = shaderInfos[index];
					if (shader == null) continue;
					this.SetTime(shader.Shader);
					bindings.Clear();
					shader.Bind(GetType(), bindings, resources);
					foreach (var k in shader.Kernels)
					{
						didDispatchAny = true;
						Debug.Log("Dispatch " + k.Name);
						shader.Dispatch(this, k.Index, bindings);
					}
				}
			}

			if (didDispatchAny)
			{
				OnAfterEvaluation();
			}
		}

		protected virtual void OnDispatched(DispatchInfo info)
		{
			
		}

		protected virtual void OnAfterEvaluation()
		{
			
		}

		private void DispatchNow(DispatchInfo info)
		{
			if (!info.IsValid) return;
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
				this.SetTime(shader.Shader);
				bindings.Clear();
				shader.Bind(GetType(), bindings, resources);
				

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
				return;
			}
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
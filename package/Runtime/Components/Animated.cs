using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Needle.Timeline
{
	public abstract class Animated : MonoBehaviour, IAnimated, IAnimatedEvents
	{
		private void OnValidate()
		{
			InternalInit();
		}

		private void OnEnable()
		{
			InternalInit();
		}

		private void InternalInit()
		{
			if (!didSearchFields)
			{
				didSearchFields = true;
				computeShaderFields.Clear();
				var type = GetType();
				var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (var field in fields)
				{
					if (typeof(ComputeShader).IsAssignableFrom(field.FieldType))
					{
						computeShaderFields.Add(field);
					}
				}
			}

			var changed = true;
			bindings.Clear();
			for (var index = 0; index < computeShaderFields.Count; index++)
			{
				var cs = computeShaderFields[index];
				ComputeShaderInfo info;
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
		private readonly List<ComputeShaderInfo> shaderInfos = new List<ComputeShaderInfo>();
		private readonly List<ComputeShaderBinding> bindings = new List<ComputeShaderBinding>();
		private readonly IResourceProvider resources = ResourceProvider.CreateDefault();

		public void OnReset()
		{
			
		}
		
		public void OnEvaluated(FrameInfo frame)
		{
			OnUpdate();
		}
		
		private void OnUpdate()
		{
			if (!didSearchFields) InternalInit();
			for (var index = 0; index < shaderInfos.Count; index++)
			{
				var shader = shaderInfos[index];
				this.SetTime(shader.Shader);
				bindings.Clear();
				shader.Bind(GetType(), bindings, resources);
				shader.Dispatch(this, 0, bindings);
			}
		}

		public struct DispatchInfo
		{
			
		}

		protected virtual IEnumerable<DispatchInfo> OnDispatch()
		{
			
		}
	}
}
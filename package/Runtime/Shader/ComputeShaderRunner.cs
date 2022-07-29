#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Needle.Timeline.ResourceProviders;
using UnityEngine;

namespace Needle.Timeline
{
	public struct DispatchJob
	{
		
	}
	
	public class ComputeShaderRunner : IDisposable
	{
		private readonly object target;
		private readonly List<FieldInfo> computeShaderFields = new List<FieldInfo>();
		private readonly List<ComputeShaderInfo?> shaderInfos = new List<ComputeShaderInfo?>();
		private readonly List<ComputeShaderBinding> bindings = new List<ComputeShaderBinding>();
		private readonly IResourceProvider resources = ResourceProvider.CreateDefault();
		private bool requireBinding = true;
		
		public ComputeShaderRunner(object target)
		{
			this.target = target ?? throw new ArgumentNullException(nameof(target));
			ComputeShaderUtils.ComputeShaderChanged += OnShaderChanged;
			Bind();
		}

		public void Dispose()
		{
			ComputeShaderUtils.ComputeShaderChanged -= OnShaderChanged;
			resources.Dispose();
		}

		public IEnumerable<DispatchJob> Dispatch()
		{
			if (requireBinding) Bind();
			yield break;
		}

		private void OnShaderChanged(ComputeShader obj)
		{
			if (!obj) return;
			if (shaderInfos.Any(s => s != null && s.Shader == obj))
			{
				requireBinding = true;
			} 
		}

		private void Bind()
		{
			requireBinding = false;
			computeShaderFields.Clear();
			var type = target.GetType();
			var fields = type.GetFields(BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (var field in fields)
			{
				if (typeof(ComputeShader).IsAssignableFrom(field.FieldType))
				{
					computeShaderFields.Add(field);
				}
			}
			
			if (computeShaderFields.Count <= 0)
			{
				Debug.LogWarning("No ComputeShader field found in " + type.FullName);
			}
			
			bindings.Clear();
			for (var index = 0; index < computeShaderFields.Count; index++)
			{
				var cs = computeShaderFields[index];
				ComputeShaderInfo? info;
				var shader = cs.GetValue(this) as ComputeShader;

				if (shader)
				{
					shader.StartWatching(() => requireBinding = true);
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
		}
	}
}
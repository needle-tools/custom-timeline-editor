#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Needle.Timeline
{
	public class ComputeShaderBinding
	{
		public object? Instance;
		public FieldInfo TypeField;
		public ComputeShaderFieldInfo ShaderField;
		public ComputeShaderInfo ShaderInfo;

		public ComputeShaderBinding(object? instance, FieldInfo typeField, ComputeShaderFieldInfo shaderField, ComputeShaderInfo shaderInfo)
		{
			Instance = instance;
			TypeField = typeField;
			ShaderField = shaderField;
			ShaderInfo = shaderInfo;
		}
	}
	
	public static partial class ComputeShaderUtils
	{
		public static bool SetValues(this ComputeShaderInfo shaderInfo, object source, List<ComputeShaderBinding>? bindings = null)
		{
			var type = source.GetType();
			var fieldInType = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
			foreach (var shaderField in shaderInfo.Fields)
			{
				var found = false;
				foreach (var typeField in fieldInType)
				{
					if (typeField.Name == shaderField.FieldName)
					{
						found = true;

						throw new NotImplementedException("TODO");
						
						if (bindings != null)
						{
							var binding = new ComputeShaderBinding(source, typeField, shaderField, shaderInfo);
							bindings.Add(binding);
						}
					}
				}
				if (!found)
				{
					Debug.LogWarning("Did not find " + shaderField);
				}
			}
			return true;
		}
	}
}
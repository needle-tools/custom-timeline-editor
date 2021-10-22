#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

// ReSharper disable ReplaceWithSingleAssignment.False

namespace Needle.Timeline
{
	public static partial class ComputeShaderUtils
	{
		public static bool Dispatch(this ComputeShaderInfo shaderInfo,
			object instance,
			int kernelIndex,
			List<ComputeShaderBinding> bindings,
			Vector3Int? kernelGroupSize = null)
		{
			if (shaderInfo.HasError)
			{
				Debug.LogError(shaderInfo.Error);
				return false;
			}
			foreach (var k in shaderInfo.Kernels)
			{
				if (k.Index == kernelIndex)
				{
					if (bindings != null && bindings.Count > 0)
					{
						foreach (var b in bindings)
						{
							if (b.ShaderField.Kernels?.Any(x => x.Name == k.Name) ?? false)
							{
								b.Bind(instance);
								if (!b.SetValue(kernelIndex))
								{
									Debug.LogWarning("Failed setting " + b.ShaderField.TypeName + " " + b.ShaderField.FieldName);
								}
							}
						}
						var threads = k.Threads;
						if (kernelGroupSize != null)
						{
							var gs = kernelGroupSize.Value;
							if (gs.x > 0)
								threads.x = Mathf.CeilToInt(gs.x / (float)threads.x);
							if (gs.y > 0)
								threads.y = Mathf.CeilToInt(gs.y / (float)threads.y);
							if (gs.z > 0)
								threads.z = Mathf.CeilToInt(gs.z / (float)threads.z);
						}
						// Debug.Log($"Dispatch {k.Name} with {threads} threads");

						shaderInfo.Shader.Dispatch(k.Index, threads.x, threads.y, threads.z);
						return true;
					}
				}
			}
			return false;
		}

		public static readonly (string fieldName, string typeName)[] BuiltinTypeNames = 
		{
			("_Time", "float4")
		};

		public static bool Bind(this ComputeShaderInfo shaderInfo, Type type, List<ComputeShaderBinding> bindings, 
			IResourceProvider resources)
		{
			if (bindings == null) throw new ArgumentNullException(nameof(bindings));
			var success = true;
			var fieldInType = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
			foreach (var shaderField in shaderInfo.Fields)
			{
				var found = false;
				foreach (var typeField in fieldInType)
				{
					if (found) break;
					if (typeField.Name != shaderField.FieldName)
					{
						var mapping = typeField.GetCustomAttribute<ShaderField>();
						if (mapping == null || mapping.Name == null || mapping.Name != shaderField.FieldName)
							continue;
					}

					var manual = typeField.GetCustomAttribute<Manual>();
					if (manual != null && typeField.FieldType == shaderField.FieldType)
					{
						// nothing to be done here, we expect the user to set this field before setting it to the shader
					}
					else
					{
						var stride = 0;
						if (typeof(ComputeBuffer).IsAssignableFrom(typeField.FieldType))
						{
							var attr = typeField.GetCustomAttribute<ComputeBufferInfo>();
							if (attr == null)
							{
								success = false;
								Debug.LogWarning($"Missing {nameof(ComputeBufferInfo)} attribute on {typeField.DeclaringType?.Name}.{typeField.Name}");
								continue;
							}
							stride = attr.Stride;
						}
						else
							stride = typeField.FieldType.GetStride();
					
						if (stride != shaderField.Stride)
						{
							var handledStrideMismatch = false;
							if (typeof(Texture).IsAssignableFrom(typeField.FieldType))
							{
								handledStrideMismatch = true;
							}

							if (!handledStrideMismatch)
							{
								success = false;
								Debug.LogError($"Found unknown stride mismatch: {typeField.Name} ({stride}) != {shaderField.FieldName} ({shaderField.Stride})");
								continue;
							}
						}
					}
					
					found = true;
					var binding = new ComputeShaderBinding(typeField, shaderField, shaderInfo, resources);
					bindings.Add(binding);
				}
				if (!found)
				{
					if (!BuiltinTypeNames.Any(e => 
						    e.fieldName == shaderField.FieldName && e.typeName == shaderField.TypeName
						    ))
					{ 
						success = false;
						Debug.LogWarning("Did not find " + shaderField);
					}
				}
			}

			return success;
		}
	}
}
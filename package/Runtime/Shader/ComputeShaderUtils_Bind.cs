#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Needle.Timeline.ResourceProviders;
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
								if (!b.SetValue(instance, kernelIndex))
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

		public static bool Bind(this ComputeShaderInfo shaderInfo,
			Type type,
			List<ComputeShaderBinding> bindings,
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
					if (TryBind(shaderField, typeField, shaderInfo, resources, out var binding))
					{
						bindings.Add(binding);
						found = true;
					}
					else success = false;
				}
				if (!found)
				{
					if (!BuiltinTypeNames.Any(e =>
						    e.fieldName == shaderField.FieldName && e.typeName == shaderField.TypeName
					    ))
					{
						success = false;
						Debug.LogWarning(type + " has no matching field for shader field: " + shaderField);
					}
				}
			}

			return success;
		}

		private static bool TryBind(ComputeShaderFieldInfo shaderField, FieldInfo typeField, ComputeShaderInfo shaderInfo, IResourceProvider resources, out ComputeShaderBinding binding)
		{
			binding = null!;
			
			if (typeField.Name != shaderField.FieldName)
			{
				var mapping = typeField.GetCustomAttribute<ShaderField>();
				if (mapping == null || mapping.Name == null || mapping.Name != shaderField.FieldName)
					return false;
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
						Debug.LogWarning($"Missing {nameof(ComputeBufferInfo)} attribute on {typeField.DeclaringType?.Name}.{typeField.Name}");
						return false;
					}
					stride = attr.Stride;
				}
				else if (typeof(Texture).IsAssignableFrom(typeField.FieldType))
				{
					var info = typeField.GetCustomAttribute<TextureInfo>();
					if (info == null)
					{
						Debug.LogWarning($"Missing {nameof(TextureInfo)} attribute on {typeField.DeclaringType?.Name}.{typeField.Name}");
						return false;
					}
				}
				else if (typeof(Transform).IsAssignableFrom(typeField.FieldType))
				{
					// if user tries to bind a transform to a shaderfield allow it for certain types
					switch (shaderField.TypeName)
					{
						case "float3":
						case "float4":
						case "float4x4":
							stride = shaderField.Stride;
							break;
					}
				}
				else if (typeof(IList<Transform>).IsAssignableFrom(typeField.FieldType))
				{
					switch (shaderField.GenericTypeName) 
					{
						case "float3":
						case "float4":
						case "float4x4":
							stride = shaderField.Stride;
							break;
					}
				}
				else 
				{
					stride = typeField.FieldType.GetStride();
				}

				if (stride != shaderField.Stride)
				{
					var handledStrideMismatch = false;
					if (typeof(Texture).IsAssignableFrom(typeField.FieldType))
					{
						handledStrideMismatch = true;
					}

					if (!handledStrideMismatch)
					{
						Debug.LogError(
							$"Found unknown stride mismatch: Field.{typeField.Name} ({stride}) != Shader.{shaderField.FieldName} ({shaderField.Stride})");
						return false;
					}
				}
			}

			var b = ShaderBridgeBuilder.BuildMapping(typeField);
			binding = new ComputeShaderBinding(b, typeField, shaderField, shaderInfo, resources);
			return true;
		}
	}
}
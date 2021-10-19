#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Reflection;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

// ReSharper disable ReplaceWithSingleAssignment.False

namespace Needle.Timeline
{
	public class ComputeShaderBinding
	{
		public object? Instance;
		public readonly FieldInfo TypeField;
		public readonly ComputeShaderFieldInfo ShaderField;
		public readonly ComputeShaderInfo ShaderInfo;
		public readonly IResourceProvider Resources;

		public ComputeShaderBinding(FieldInfo typeField, ComputeShaderFieldInfo shaderField, ComputeShaderInfo shaderInfo, IResourceProvider resourceProvider)
		{
			TypeField = typeField;
			ShaderField = shaderField;
			ShaderInfo = shaderInfo;
			Resources = resourceProvider;
		}

		private static FieldInfo? list_backingArray;

		public void Bind(object instance)
		{
			this.Instance = instance;
		}
		
		public bool SetValue(int kernelIndex)
		{
			var value = TypeField.GetValue(Instance);
			
			if (value is IList list)
			{
				var buffer = Resources.ComputeBufferProvider.GetBuffer(ShaderField.FieldName, list.Count, ShaderField.Stride,
					ShaderField.RandomWrite.GetValueOrDefault() ? ComputeBufferType.Structured : ComputeBufferType.Default);
				if (list is Array arr) buffer.SetData(arr);
				else
				{
					// TODO: find better way of setting content to buffer
					list_backingArray ??= list.GetType().GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic);
					var backingArray = list_backingArray.GetValue(list) as Array;
					buffer.SetData(backingArray, 0, 0, list.Count);
				}
				ShaderInfo.Shader.SetBuffer(kernelIndex, ShaderField.FieldName, buffer);
				return true;
			}

			if (typeof(Texture).IsAssignableFrom(TypeField.FieldType))
			{
				var info = TypeField.GetCustomAttribute<TextureInfo>();
				if (info != null)
				{
					if (value == null)
					{
						GraphicsFormat? graphicsFormat = info.GraphicsFormat;
						if ((int)graphicsFormat == 0 && (int)info.TextureFormat != 0) 
							graphicsFormat = GraphicsFormatUtility.GetGraphicsFormat(info.TextureFormat, false);
						var rt = Resources.RenderTextureProvider.GetTexture(TypeField.Name, info.Width, info.Height, 
							info.Depth.GetValueOrDefault(), graphicsFormat);
						value = rt;
						ShaderInfo.Shader.SetTexture(kernelIndex, ShaderField.FieldName, rt);
						TypeField.SetValue(Instance, value);
						return true;
					}
				}
			}
			
			switch (value)
			{
				case float val:
					ShaderInfo.Shader.SetFloat(ShaderField.FieldName, val);
					break;
				case Vector2 val:
					ShaderInfo.Shader.SetVector(ShaderField.FieldName, val);
					break;
				case Vector3 val:
					ShaderInfo.Shader.SetVector(ShaderField.FieldName, val);
					break;
				case Vector4 val:
					ShaderInfo.Shader.SetVector(ShaderField.FieldName, val);
					break;
				case int val:
					ShaderInfo.Shader.SetInt(ShaderField.FieldName, val);
					break;
				case uint val:
					ShaderInfo.Shader.SetInt(ShaderField.FieldName, (int)val);
					break;
				case Vector2Int val:
					ShaderInfo.Shader.SetVector(ShaderField.FieldName, (Vector2)val);
					break;
				case Vector3Int val:
					ShaderInfo.Shader.SetVector(ShaderField.FieldName, (Vector3)val);
					break;
				case Matrix4x4 val:
					ShaderInfo.Shader.SetMatrix(ShaderField.FieldName, val);
					break;
				default:
					return false;
			}
			return true;
		}

		public object GetValue()
		{
			if (typeof(IList).IsAssignableFrom(TypeField.FieldType))
			{
				var list = TypeField.GetValue(Instance) as IList;
				var buffer = Resources.ComputeBufferProvider.GetBuffer(ShaderField.FieldName, list!.Count, ShaderField.Stride, 
					ShaderField.RandomWrite.GetValueOrDefault() ? ComputeBufferType.Structured : ComputeBufferType.Default);
				var arr = Array.CreateInstance(list.GetType().GetGenericArguments().First(), list.Count);
				buffer.GetData(arr, 0, 0, list.Count);
				return arr;
			}
			return null;
		}

		// internal void Assert()
		// {
		// 	var value = TypeField.GetValue(Instance);
		// 	SetValue();
		// 	ShaderInfo.Shader.Dispatch(0, 1, 1, 1);
		// 	if (value is IList list)
		// 	{
		// 		var buffer = Resources.ComputeBufferProvider.GetBuffer(ShaderField.FieldName, list.Count, ShaderField.Stride, 
		// 			ShaderField.RandomWrite.GetValueOrDefault() ? ComputeBufferType.Structured : ComputeBufferType.Default);
		// 		var arr = Array.CreateInstance(list.GetType().GetGenericArguments().First(), list.Count);
		// 		buffer.GetData(arr);
		// 		Debug.Assert(arr.Length == list.Count);
		// 		Debug.Assert(arr.GetValue(0) != null);
		// 		Debug.Assert(Equals(arr.GetValue(0), list[0]));
		// 	}
		// }
	}
	
	public static partial class ComputeShaderUtils
	{
		public static void Dispatch(this ComputeShaderInfo shaderInfo, object instance, int kernelIndex, List<ComputeShaderBinding> bindings, Vector3Int? kernelGroupSize = null)
		{
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
									Debug.LogError("Failed setting " + b.ShaderField.TypeName + " " + b.ShaderField.FieldName);
								}
							}
						}
						var threads = k.Threads;
						if (kernelGroupSize != null)
						{
							threads.x = Mathf.CeilToInt((float)threads.x / kernelGroupSize.Value.x);
							threads.y = Mathf.CeilToInt((float)threads.y / kernelGroupSize.Value.y);
							threads.z = Mathf.CeilToInt((float)threads.z / kernelGroupSize.Value.z);
						}
						shaderInfo.Shader.Dispatch(k.Index, threads.x, threads.y, threads.z);
					}
				}
			}
		}

		public static bool Bind(this ComputeShaderInfo shaderInfo, Type type, List<ComputeShaderBinding> bindings, IResourceProvider resources)
		{
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
						if(mapping == null || mapping.Name == null || mapping.Name != shaderField.FieldName)
							continue;
					}
					
					var stride = typeField.FieldType.GetStride();
					if (stride != shaderField.Stride)
					{
						var handledStrideMismatch = false;
						if (typeof(Texture).IsAssignableFrom(typeField.FieldType))
						{
							handledStrideMismatch = true;
						}

						if (!handledStrideMismatch)
						{
							Debug.LogError($"Found unknown stride mismatch: {typeField.Name} ({stride}) != {shaderField.FieldName} ({shaderField.Stride})");
							continue;
						}
					}
					found = true;

					// throw new NotImplementedException("TODO");
						
					if (bindings != null)
					{
						var binding = new ComputeShaderBinding(typeField, shaderField, shaderInfo, resources);
						bindings.Add(binding);
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
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

namespace Needle.Timeline
{
	public class ComputeShaderBinding
	{
		public object? Instance;
		public readonly FieldInfo TypeField;
		public readonly ComputeShaderFieldInfo ShaderField;
		public readonly ComputeShaderInfo ShaderInfo;
		public readonly IResourceProvider Resources;

		public ComputeShaderBinding(object? instance, FieldInfo typeField, ComputeShaderFieldInfo shaderField, ComputeShaderInfo shaderInfo, IResourceProvider resourceProvider)
		{
			Instance = instance;
			TypeField = typeField;
			ShaderField = shaderField;
			ShaderInfo = shaderInfo;
			Resources = resourceProvider;
		}

		private static FieldInfo? list_backingArray;
		
		public void SetValue()
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
				ShaderInfo.Shader.SetBuffer(ShaderField.Kernels!.First().Index, ShaderField.FieldName, buffer);
			}
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

		internal void Assert()
		{
			var value = TypeField.GetValue(Instance);
			SetValue();
			ShaderInfo.Shader.Dispatch(0, 1, 1, 1);
			if (value is IList list)
			{
				var buffer = Resources.ComputeBufferProvider.GetBuffer(ShaderField.FieldName, list.Count, ShaderField.Stride, 
					ShaderField.RandomWrite.GetValueOrDefault() ? ComputeBufferType.Structured : ComputeBufferType.Default);
				var arr = Array.CreateInstance(list.GetType().GetGenericArguments().First(), list.Count);
				buffer.GetData(arr);
				Debug.Assert(arr.Length == list.Count);
				Debug.Assert(arr.GetValue(0) != null);
				Debug.Assert(Equals(arr.GetValue(0), list[0]));
			}
		}
	}
	
	public static partial class ComputeShaderUtils
	{
		public static bool Bind(this ComputeShaderInfo shaderInfo, object source, List<ComputeShaderBinding> bindings, IResourceProvider resources)
		{
			var type = source.GetType();
			var fieldInType = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
			foreach (var shaderField in shaderInfo.Fields)
			{
				var found = false;
				foreach (var typeField in fieldInType)
				{
					if (typeField.Name != shaderField.FieldName)
					{
						var mapping = typeField.GetCustomAttribute<ShaderField>();
						if(mapping == null || mapping.Name == null || mapping.Name != shaderField.FieldName)
							continue;
					}
					
					var stride = typeField.FieldType.GetStride();
					if (stride != shaderField.Stride)
					{
						Debug.LogError("Found stride mismatch: " + typeField.Name + " != " + shaderField.FieldName + " - TODO: Handle properly?");
						continue;
					}
					found = true;

					// throw new NotImplementedException("TODO");
						
					if (bindings != null)
					{
						var binding = new ComputeShaderBinding(source, typeField, shaderField, shaderInfo, resources);
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
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

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

		private FieldInfo? list_backingArray;

		public void Bind(object instance)
		{
			Instance = instance;
		}

		public bool SetValue(int kernelIndex)
		{
			var value = TypeField.GetValue(Instance);

			if (typeof(ComputeBuffer).IsAssignableFrom(TypeField.FieldType))
			{
				var buffer = value as ComputeBuffer;
				if (buffer == null || !buffer.IsValid())
				{
					var info = TypeField.GetCustomAttribute<ComputeBufferInfo>();
					buffer = Resources.ComputeBufferProvider.GetBuffer(ShaderField.FieldName, info.Size, info.Stride, info.Type, info.Mode);
					buffer.name = TypeField.Name;
				}
				ShaderInfo.Shader.SetBuffer(kernelIndex, ShaderField.FieldName, buffer);
				TypeField.SetValue(Instance, buffer);
				return true;
			}

			// TODO: handle when list is null
			if (typeof(IList).IsAssignableFrom(TypeField.FieldType))
			{
				var list = value as IList;
				if (list == null)
				{
				}
				else
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
			}

			if (typeof(Transform).IsAssignableFrom(TypeField.FieldType))
			{
				if (value == null)
					return false;
				var t = (Transform)value;
				var name = ShaderField.FieldName;
				switch (ShaderField.TypeName)
				{
					case "float3":
					case "float4":
						// Debug.Log("Set " + t.position);
						ShaderInfo.Shader.SetVector(name, t.position);
						return true;
					case "float4x4":
						ShaderInfo.Shader.SetMatrix(name, t.localToWorldMatrix);
						return true;
					default:
						return false;
				}
			}

			if (typeof(Texture).IsAssignableFrom(TypeField.FieldType))
			{
				var info = TypeField.GetCustomAttribute<TextureInfo>();
				if (info != null)
				{
					// if (value == null)
					{
						GraphicsFormat? graphicsFormat = info.GraphicsFormat;
						if ((int)graphicsFormat == 0 )
						{
							if((int)info.TextureFormat != 0)
								graphicsFormat = GraphicsFormatUtility.GetGraphicsFormat(info.TextureFormat, false);
							else
							{
								if (ShaderField.GenericTypeName == null) 
									throw new Exception("Failed finding generic type: " + ShaderField.FieldName);
								switch (ShaderField.GenericTypeName)
								{
									case "float":
										graphicsFormat = GraphicsFormat.R16_SFloat;
										break;
									case "float2":
										graphicsFormat = GraphicsFormat.R16G16_SFloat;
										break;
									case "float3":
										graphicsFormat = GraphicsFormat.R16G16B16_SFloat;
										break;
									case "float4":
										graphicsFormat = GraphicsFormat.R16G16B16A16_SFloat;
										break;
								}
							}
						}
						
						var rt = Resources.RenderTextureProvider.GetTexture(TypeField.Name, info.Width, info.Height,
							info.Depth.GetValueOrDefault(), graphicsFormat, ShaderField.RandomWrite);
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

		public object? GetValue()
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
	}
}
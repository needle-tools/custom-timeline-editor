﻿using System;
using System.Reflection;
using Needle.Timeline.ResourceProviders;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Object = UnityEngine.Object;

namespace Needle.Timeline
{
	[ShaderBridge(typeof(Texture))]
	public class TextureBridge : IShaderBridge
	{
		private TextureInfo info;

		private static GraphicsFormat GetFormat(string typeName)
		{
			switch (typeName)
			{
				case "float":
					return GraphicsFormat.R16_SFloat;
				case "float2":
					return GraphicsFormat.R16G16_SFloat;
				case "float3":
					return GraphicsFormat.R16G16B16_SFloat;
				case "float4":
					return GraphicsFormat.R16G16B16A16_SFloat;
			}
			return GraphicsFormat.None;
		}

		public bool SetValue(IBindingContext context)
		{
			var field = context.Field;
			var shaderField = context.ShaderField;

			info ??= field.GetCustomAttribute<TextureInfo>();
			var value = field.GetValue(context.Instance);
			var tex = value as Texture;
			var renderTex = value as RenderTexture;

			if (renderTex && (info == null || !info.HasValidSize))
			{
			}
			else
			{
				if (value == null || !(Object)value 
				                  || renderTex && (renderTex.width != info.Width || renderTex.height != info.Height || renderTex.enableRandomWrite != shaderField.RandomWrite)
				                  )
				{
					
					var type = value?.GetType();
					if (type == typeof(Texture2D) || type == typeof(Texture3D))
					{
						Debug.LogError("Can not create type: " + type);
					}
					else
					{
						GraphicsFormat? graphicsFormat = info.GraphicsFormat;
						if ((int)graphicsFormat == 0)
						{
							if ((int)info.TextureFormat != 0)
								graphicsFormat = GraphicsFormatUtility.GetGraphicsFormat(info.TextureFormat, false);
							else
							{
								if (shaderField.GenericTypeName == null)
									throw new Exception("Failed finding generic type: " + shaderField.FieldName);
								graphicsFormat = GetFormat(shaderField.GenericTypeName);
							}
						}

						var desc = info.ToRenderTextureDescription();
						desc.Name = field.Name;
						desc.RandomAccess = shaderField.RandomWrite.GetValueOrDefault();
						desc.GraphicsFormat = graphicsFormat;
						var rt = context.Resources.RenderTextureProvider.GetTexture(field.Name, desc);
						value = tex = rt;
						field.SetValue(context.Instance, value);
					}
					
				}
			}


			if (tex)
			{
				context.ShaderInfo.Shader.SetTexture(context.KernelIndex, shaderField.FieldName, tex);
				return true;
			}

			return false;
		}
	}
}
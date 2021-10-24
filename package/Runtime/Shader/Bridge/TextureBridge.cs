﻿using System;
using System.Reflection;
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
		
		public bool SetValue(IBindingContext context)
		{
			var field = context.Field;
			var shaderField = context.ShaderField;
			
			info ??= field.GetCustomAttribute<TextureInfo>();
			var value = field.GetValue(context.Instance);
			var tex = value as Texture;
			var renderTex = value as RenderTexture;
			if (value == null || !(Object)value || renderTex && (renderTex.width != info.Width || renderTex.height != info.Height))
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
						switch (shaderField.GenericTypeName)
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

				var rt = context.Resources.RenderTextureProvider.GetTexture(field.Name, info.Width, info.Height,
					info.Depth, graphicsFormat, shaderField.RandomWrite, rt =>
					{
						rt.filterMode = info.FilterMode;
					});
				value = tex = rt; 
				field.SetValue(context.Instance, tex);
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
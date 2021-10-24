using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Needle.Timeline
{
	public class TextureBridge : IShaderBridge
	{
		public bool SetValue(IBindingContext context)
		{
			var field = context.Field;
			var shaderField = context.ShaderField;
			var info = field.GetCustomAttribute<TextureInfo>();
			var value = field.GetValue(context.Instance);

			if (value == null)
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
					info.Depth.GetValueOrDefault(), graphicsFormat, shaderField.RandomWrite);
				value = rt;
				field.SetValue(context.Instance, value);
			}

			if (value is Texture tex)
			{
				context.ShaderInfo.Shader.SetTexture(context.KernelIndex, shaderField.FieldName, tex);
				return true;
			}
			
			return false;
		}
	}
}
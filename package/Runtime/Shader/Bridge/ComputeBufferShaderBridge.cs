﻿using System.Reflection;
using Needle.Timeline.ResourceProviders;
using UnityEngine;

namespace Needle.Timeline
{
	[ShaderBridge(typeof(ComputeBuffer))]
	public class ComputeBufferBridge : IShaderBridge
	{
		public bool SetValue(IBindingContext context)
		{
			var instance = context.Instance;
			var field = context.Field;
			var shaderField = context.ShaderField;
			var resources = context.Resources;
			var shaderInfo = context.ShaderInfo;
			var value = field.GetValue(instance);

			var buffer = value as ComputeBuffer;
			if (buffer == null || !buffer.IsValid())
			{
				var info = field.GetCustomAttribute<ComputeBufferInfo>();
				var desc = shaderField.GetComputeBufferDescription();
				desc.Size = info.Size;
				desc.Type = info.Type;
				desc.Mode = info.Mode;
				
				buffer = resources.ComputeBufferProvider.GetBuffer(shaderField.FieldName, desc);
				buffer.name = field.Name;
				field.SetValue(instance, buffer);
			}
			shaderInfo.Shader.SetBuffer(context.KernelIndex, shaderField.FieldName, buffer);
			return true;
		}
	}
}
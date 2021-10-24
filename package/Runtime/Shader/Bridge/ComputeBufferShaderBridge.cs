using System.Reflection;
using UnityEngine;

namespace Needle.Timeline.Shader
{
	public class ComputeBufferShaderBridge : IShaderBridge
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
				buffer = resources.ComputeBufferProvider.GetBuffer(shaderField.FieldName, info.Size, info.Stride, info.Type, info.Mode);
				buffer.name = field.Name;
				field.SetValue(instance, buffer);
			}
			shaderInfo.Shader.SetBuffer(context.KernelIndex, shaderField.FieldName, buffer);
			return true;
		}
	}
}
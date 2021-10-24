using UnityEngine;

namespace Needle.Timeline
{
	[ShaderBridge(typeof(Transform))]
	public class TransformBridge : IShaderBridge
	{
		public bool SetValue(IBindingContext context)
		{
			var value = context.Field.GetValue(context.Instance);
			var shaderField = context.ShaderField;
			
			if (value == null)
				return false;
			var t = (Transform)value;
			var name = shaderField.FieldName;
			switch (shaderField.TypeName)
			{
				case "float3":
				case "float4":
					context.ShaderInfo.Shader.SetVector(name, t.position);
					return true;
				case "float4x4":
					context.ShaderInfo.Shader.SetMatrix(name, t.localToWorldMatrix);
					return true;
				default:
					return false;
			}
		}
	}
}
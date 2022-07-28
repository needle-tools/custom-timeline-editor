using System;
using UnityEngine;

namespace Needle.Timeline
{
	[ShaderBridge(
		typeof(bool),
		typeof(float),
		typeof(Vector2),
		typeof(Vector3),
		typeof(Vector4),
		typeof(Color),
		typeof(int),
		typeof(uint),
		typeof(Vector2Int),
		typeof(Vector3Int),
		typeof(Vector3Int),
		typeof(Matrix4x4))]
	public struct PrimitiveBridge : IShaderBridge
	{
		public bool SetValue(IBindingContext context)
		{
			var value = context.Field.GetValue(context.Instance);
			var shaderInfo = context.ShaderInfo;
			var shaderField = context.ShaderField;

			switch (value)
			{
				case bool val:
					shaderInfo.Shader.SetBool(shaderField.FieldName, val);
					break;
				case float val:
					shaderInfo.Shader.SetFloat(shaderField.FieldName, val);
					break;
				case Vector2 val:
					shaderInfo.Shader.SetVector(shaderField.FieldName, val);
					break;
				case Vector3 val:
					shaderInfo.Shader.SetVector(shaderField.FieldName, val);
					break;
				case Vector4 val:
					shaderInfo.Shader.SetVector(shaderField.FieldName, val);
					break;
				case Color val:
					shaderInfo.Shader.SetVector(shaderField.FieldName, val);
					break;
				case int val:
					shaderInfo.Shader.SetInt(shaderField.FieldName, val);
					break;
				case uint val:
					shaderInfo.Shader.SetInt(shaderField.FieldName, (int)val);
					break;
				case Vector2Int val:
					shaderInfo.Shader.SetVector(shaderField.FieldName, (Vector2)val);
					break;
				case Vector3Int val:
					shaderInfo.Shader.SetVector(shaderField.FieldName, (Vector3)val);
					break;
				case Matrix4x4 val:
					shaderInfo.Shader.SetMatrix(shaderField.FieldName, val);
					break;
				default:
					return false;
			}

			return true;
		}
	}
}
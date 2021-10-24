using System;
using System.Collections;
using System.Reflection;
using Needle.Timeline.Shader;
using UnityEngine;

namespace Needle.Timeline
{
	public class CollectionBridge : IShaderBridge
	{
		private FieldInfo list_backingArray;

		public bool SetValue(IBindingContext context)
		{
			var instance = context.Instance;
			var field = context.Field;
			var shaderField = context.ShaderField;
			var resources = context.Resources;
			var shaderInfo = context.ShaderInfo;
			var value = field.GetValue(instance);

			var list = value as IList;
			if (list == null)
			{
			}
			else
			{
				var buffer = resources.ComputeBufferProvider.GetBuffer(shaderField.FieldName, list.Count, shaderField.Stride,
					shaderField.RandomWrite.GetValueOrDefault() ? ComputeBufferType.Structured : ComputeBufferType.Default);
				if (list is Array arr) buffer.SetData(arr);
				else
				{
					// TODO: find better way of setting content to buffer
					list_backingArray ??= list.GetType().GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic);
					var backingArray = list_backingArray.GetValue(list) as Array;
					buffer.SetData(backingArray, 0, 0, list.Count);
				}
				shaderInfo.Shader.SetBuffer(context.KernelIndex, shaderField.FieldName, buffer);
				shaderInfo.Shader.SetInt(shaderField.FieldName + "Count", list.Count);
				return true;
			}
			return false;
		}
	}
}
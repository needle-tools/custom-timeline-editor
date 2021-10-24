using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Needle.Timeline
{
	[ShaderBridge(typeof(IList))]
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
				throw new NotImplementedException("Field is null but the shader requires this to be set: " + shaderField);
			}
			else  
			{
				// TODO: how can we specify WHEN a field should be set, for example: i only want to initialize a list with values and then mark dirty or something to notify that the buffer should be updated
				var buffer = resources.ComputeBufferProvider.GetBuffer(shaderField.FieldName, list.Count, shaderField.Stride,
					shaderField.RandomWrite.GetValueOrDefault() ? ComputeBufferType.Structured : ComputeBufferType.Default);
				if (list is Array arr) buffer.SetData(arr);
				else
				{
					// TODO: find better way of setting content to buffer
					// TODO: this works only for value types, and e.g. a list of transforms would fail
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
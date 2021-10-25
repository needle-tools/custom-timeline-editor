using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace Needle.Timeline
{
	[ShaderBridge(typeof(IList))]
	public class CollectionBridge : IShaderBridge
	{
		private FieldInfo list_backingArray;

		private bool? hasOnceAttribute = null;
		private bool didSet = false;

		public bool SetValue(IBindingContext context)
		{
			if (didSet) return true;

			var field = context.Field;
			if (hasOnceAttribute == null)
			{
				hasOnceAttribute = field.GetCustomAttribute<Once>() != null;
				if (hasOnceAttribute.Value) didSet = true;
			}
			
			var instance = context.Instance;
			var shaderField = context.ShaderField;
			var resources = context.Resources;
			var shaderInfo = context.ShaderInfo;
			var value = field.GetValue(instance); 

			if (value == null)
			{
				var buffer = resources.ComputeBufferProvider.GetBuffer(
					shaderField.FieldName, 1, shaderField.Stride,
					shaderField.RandomWrite.GetValueOrDefault() ? ComputeBufferType.Structured : ComputeBufferType.Default
					);
				shaderInfo.Shader.SetBuffer(context.KernelIndex, shaderField.FieldName, buffer);
				shaderInfo.Shader.SetInt(shaderField.FieldName + "Count", 0);
				return true;
			}
			
			if(value is IList list)
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
					buffer.SetData(backingArray, 0, 0, Mathf.Max(1, list.Count));
				}
				shaderInfo.Shader.SetBuffer(context.KernelIndex, shaderField.FieldName, buffer);
				shaderInfo.Shader.SetInt(shaderField.FieldName + "Count", list.Count);
				return true;
			}
			return false;
		}
	}
}
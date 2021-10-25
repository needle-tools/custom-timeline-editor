using System;
using System.Collections;
using System.Reflection;
using Needle.Timeline.ResourceProviders;
using UnityEngine;

namespace Needle.Timeline
{
	[ShaderBridge(typeof(IList))]
	public class CollectionBridge : IShaderBridge
	{
		private FieldInfo list_backingArray;

		private bool didSearchOnceAttribute = false;
		private bool hasOnceAttribute;
		private bool didSet = false;

		public bool SetValue(IBindingContext context)
		{
			if (didSet && hasOnceAttribute) return true;

			var field = context.Field;
			if (!didSearchOnceAttribute)
			{
				didSearchOnceAttribute = true;
				hasOnceAttribute = field.GetCustomAttribute<Once>() != null;
			}

			var instance = context.Instance;
			if (didSet && instance is IHasBindingState state && !state.IsDirty(field.Name))
			{
				return true;
			}
			didSet = true;
			
			var shaderField = context.ShaderField;
			var resources = context.Resources;
			var shaderInfo = context.ShaderInfo;
			var value = field.GetValue(instance);
			var desc = new ComputeBufferDescription();
			desc.Stride = shaderField.Stride;
			desc.Type = shaderField.RandomWrite.GetValueOrDefault() ? ComputeBufferType.Structured : ComputeBufferType.Default;

			if (value == null)
			{
				desc.Size = 1;
				var buffer = resources.ComputeBufferProvider.GetBuffer(shaderField.FieldName, desc);
				shaderInfo.Shader.SetBuffer(context.KernelIndex, shaderField.FieldName, buffer);
				shaderInfo.Shader.SetInt(shaderField.FieldName + "Count", 0);
				return true;
			}
			
			if(value is IList list)
			{
				// TODO: how can we specify WHEN a field should be set, for example: i only want to initialize a list with values and then mark dirty or something to notify that the buffer should be updated
				desc.Size = list.Count;
				var buffer = resources.ComputeBufferProvider.GetBuffer(shaderField.FieldName, desc);
				if (list is Array arr) buffer.SetData(arr);
				else
				{
					// TODO: find better way of setting content to buffer
					// TODO: this works only for value types, and e.g. a list of transforms would fail
					list_backingArray ??= list.GetType().GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic);
					var backingArray = list_backingArray.GetValue(list) as Array;
					if(list.Count > 0)
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
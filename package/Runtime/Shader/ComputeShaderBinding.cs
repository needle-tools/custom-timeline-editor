using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Needle.Timeline.ResourceProviders;
using UnityEngine;

namespace Needle.Timeline
{
	public class ComputeShaderBinding : IBindingContext
	{
		public object Instance { get; set; }
		public int KernelIndex { get; private set; }
		public IResourceProvider Resources { get; }
		public ComputeShaderInfo ShaderInfo { get; }
		public FieldInfo Field { get; }
		public ComputeShaderFieldInfo ShaderField { get; }

		private readonly IShaderBridge bridge;

		public ComputeShaderBinding(IShaderBridge bridge, FieldInfo typeField, ComputeShaderFieldInfo shaderField, ComputeShaderInfo shaderInfo, IResourceProvider resourceProvider)
		{
			this.bridge = bridge;
			Field = typeField;
			ShaderField = shaderField;
			ShaderInfo = shaderInfo;
			Resources = resourceProvider;
		}

		public bool SetValue(object instance, int kernelIndex)
		{
			this.Instance = instance;
			this.KernelIndex = kernelIndex;
			return bridge?.SetValue(this) ?? false;
		}

		public object? GetValue()
		{
			if (typeof(IList).IsAssignableFrom(Field.FieldType))
			{
				var list = Field.GetValue(Instance) as IList;
				var desc = ComputeBufferDescription.Default(list!.Count, ShaderField.Stride);
				if (ShaderField.RandomWrite.GetValueOrDefault())
					desc.Type = ComputeBufferType.Structured;
				else desc.Type = ComputeBufferType.Constant;
				var buffer = Resources.ComputeBufferProvider.GetBuffer(ShaderField.FieldName, desc);
				var arr = Array.CreateInstance(list.GetType().GetGenericArguments().First(), list.Count);
				buffer.GetData(arr, 0, 0, list.Count);
				return arr;
			}
			return null;
		}
	}
}
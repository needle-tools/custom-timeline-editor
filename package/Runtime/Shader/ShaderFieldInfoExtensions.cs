using Needle.Timeline.ResourceProviders;
using UnityEngine;

namespace Needle.Timeline
{
	public static class ShaderFieldInfoExtensions
	{
		public static ComputeBufferDescription GetComputeBufferDescription(this ComputeShaderFieldInfo field)
		{
			var desc = new ComputeBufferDescription();
			desc.Name = field.FieldName;
			desc.Type = field.RandomWrite.GetValueOrDefault() ? ComputeBufferType.Structured : ComputeBufferType.Default;
			desc.Stride = field.Stride;
			return desc;
		}
	}
}
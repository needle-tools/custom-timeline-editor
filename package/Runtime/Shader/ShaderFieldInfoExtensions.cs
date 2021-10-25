using Needle.Timeline.ResourceProviders;
using UnityEngine;

namespace Needle.Timeline
{
	public static class ShaderFieldInfoExtensions
	{
		public static ComputeBufferDescription GetDescription(this ComputeShaderFieldInfo field)
		{
			var desc = new ComputeBufferDescription();
			desc.Name = field.FieldName;
			desc.Type = field.RandomWrite.GetValueOrDefault() ? ComputeBufferType.Structured : ComputeBufferType.Default;
			return desc;
		}
	}
}
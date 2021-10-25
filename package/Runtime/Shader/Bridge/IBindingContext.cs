using System.Reflection;
using Needle.Timeline.ResourceProviders;

namespace Needle.Timeline
{
	public interface IBindingContext
	{
		object Instance { get; }
		int KernelIndex { get; }
		IResourceProvider Resources { get; }
		ComputeShaderInfo ShaderInfo { get; }
		FieldInfo Field { get; }
		ComputeShaderFieldInfo ShaderField { get; }
	}

	public struct BindingContext : IBindingContext
	{
		public BindingContext(object instance, int kernelIndex, IResourceProvider resources, 
			ComputeShaderInfo shaderInfo, FieldInfo field, ComputeShaderFieldInfo shaderField)
		{
			Instance = instance;
			KernelIndex = kernelIndex;
			Resources = resources;
			ShaderInfo = shaderInfo;
			Field = field;
			ShaderField = shaderField;
		}

		public object Instance { get; }
		public int KernelIndex { get; }
		public IResourceProvider Resources { get; }
		public ComputeShaderInfo ShaderInfo { get; }
		public FieldInfo Field { get; }
		public ComputeShaderFieldInfo ShaderField { get; }
	}
}
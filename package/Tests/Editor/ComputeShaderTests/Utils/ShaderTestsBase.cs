using Needle.Timeline.ResourceProviders;
using NUnit.Framework;
using UnityEngine;

namespace Needle.Timeline.Tests
{
	public class ShaderTestsBase
	{
		
		protected IResourceProvider TestsResourceProvider => new ResourceProvider(new DefaultComputeBufferProvider(), new DefaultRenderTextureProvider());

		protected ComputeShader LoadShader(string name)
		{
			var shader = Resources.Load<ComputeShader>(name);
			Assert.NotNull(shader);
			return shader;
		}
	}
}
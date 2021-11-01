using System.Collections.Generic;
using NUnit.Framework;

namespace Needle.Timeline.Tests.Bind_ComputeShaderTests
{
	public class RemappingTests : ShaderTestsBase
	{
		private class MappingType
		{
			[ShaderField("MyBuffer")]
			public readonly List<float> MyList = new List<float>();
		}
		
		[Test]
		public void RemapBuffer()
		{
			var shader = LoadShader("SetValues/BufferWithFloat");
			shader.TryParse(out var shaderInfo);
			Assert.NotNull(shaderInfo);

			var list = new List<ComputeShaderBinding>();
			shaderInfo.Bind(typeof(MappingType), list, TestsResourceProvider);
			Assert.AreEqual(1, list.Count);
			Assert.IsTrue(list[0].Field.Name == nameof(MappingType.MyList));
			Assert.IsTrue(list[0].ShaderField.FieldName == "MyBuffer");
		}
	}
}
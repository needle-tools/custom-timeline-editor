using NUnit.Framework;
using UnityEngine;

namespace Needle.Timeline.Tests.Bind_ComputeShaderTests
{
	public class BufferTests : ShaderTestsBase
	{
		
		
		[Test]
		public void BufferWithStruct()
		{
			var shader = LoadShader("BufferWithStruct");
			shader.TryParse(out var shaderInfo);

			Debug.Log(shaderInfo);

			Assert.NotNull(shaderInfo);
			shaderInfo.AssertDefaults();
			Assert.AreEqual(1, shaderInfo.Kernels.Count);
			Assert.AreEqual(1, shaderInfo.Fields.Count);
			Assert.AreEqual(1, shaderInfo.Structs.Count);
			Assert.AreEqual("BufferWithStruct", shaderInfo.Fields[0].FieldName);
			Assert.AreNotEqual(-1, shaderInfo.Fields[0].Stride);
			Assert.AreEqual(4, shaderInfo.Fields[0].Stride);
			Assert.NotNull(shaderInfo.Fields[0].GenericType, "Failed finding buffer generic type");
			Assert.AreEqual("MyType", shaderInfo.Fields[0].GenericType.Name);
		}


	}
}
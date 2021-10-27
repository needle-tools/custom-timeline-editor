using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Needle.Timeline.Tests.Bind_ComputeShaderTests
{
	public class FieldTests : ShaderTestsBase
	{
		
		public class TypeWithMissingField{}
		
		[Test]
		public void Bind_FailIfShaderFieldIsMissing()
		{
			var shader = LoadShader("ShaderWithOneField");
			shader.TryParse(out var shaderInfo);
			Assert.NotNull(shaderInfo);
			var success = shaderInfo.Bind(typeof(TypeWithMissingField), new List<ComputeShaderBinding>(), TestsResourceProvider);
			Assert.AreEqual(false, success, "Expected binding to fail");
		}
		[Test]
		public void ShaderWithFieldsAndStruct()
		{
			var shader = LoadShader("ShaderWithFieldsAndStruct");
			shader.TryParse(out var shaderInfo);

			Assert.NotNull(shaderInfo);
			shaderInfo.AssertDefaults();
			Assert.AreEqual( 1, shaderInfo.Kernels.Count);
			Assert.AreEqual(7, shaderInfo.Fields.Count);
			Assert.AreEqual(1, shaderInfo.Structs.Count);
			Assert.AreEqual(12, shaderInfo.Structs[0].CalcStride());
			Assert.AreEqual(2, shaderInfo.Structs[0].Fields.Count);
		}

		[Test]
		public void FieldUsedInKernel()
		{
			var shader = LoadShader("FieldUsedInKernel");
			shader.TryParse(out var shaderInfo);

			Debug.Log(shaderInfo);

			Assert.NotNull(shaderInfo);
			shaderInfo.AssertDefaults();
			Assert.NotNull(shaderInfo.Fields[0].Kernels, "Failed finding kernel for " + shaderInfo.Fields[0]);
			Assert.NotNull(shaderInfo.Fields[1].Kernels, "Failed finding kernel for " + shaderInfo.Fields[1]);
			Assert.IsTrue(shaderInfo.Fields[0].Kernels![0].Name == "CSMain");
			Assert.IsTrue(shaderInfo.Fields[1].Kernels![0].Name == "CSMain");
		}

		[Test]
		public void FieldUsedInKernel_2()
		{
			var shader = LoadShader("FieldUsedInKernel_2");
			shader.TryParse(out var shaderInfo);

			Debug.Log(shaderInfo);

			Assert.NotNull(shaderInfo);
			shaderInfo.AssertDefaults();
			Assert.AreEqual(3, shaderInfo.Fields.Count);
			foreach (var field in shaderInfo.Fields)
			{
				Assert.NotNull(field.Kernels, "Did not find kernel for: " + field.FieldName);
				foreach (var k in field.Kernels!)
				{
					Assert.IsTrue(k.Name == "CSMain", "Field " + field.FieldName + " not found in kernel");
				}
			}
		}
		[Test]
		public void FieldNotUsedInKernel()
		{
			var shader = LoadShader("FieldNotUsedInKernel");
			shader.TryParse(out var shaderInfo);

			Debug.Log(shaderInfo);

			Assert.NotNull(shaderInfo);
			shaderInfo.AssertDefaults();
			Assert.IsNull(shaderInfo.Fields[0].Kernels, "Is not actually used: " + shaderInfo.Fields[0].FieldName);
			Assert.NotNull(shaderInfo.Fields[1].Kernels, "Failed finding kernel for " + shaderInfo.Fields[1]);
			Assert.IsTrue(shaderInfo.Fields[1].Kernels![0].Name == "CSMain");
		}

	}
}
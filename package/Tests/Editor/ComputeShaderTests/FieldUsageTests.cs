using System.Linq;
using NUnit.Framework;

namespace Needle.Timeline.Tests.Bind_ComputeShaderTests
{
	public class FieldUsageTests : ShaderTestsBase
	{
		private UsageType BaseTest(string shaderName, bool expectFindingUsage = true)
		{
			var shader = LoadShader(shaderName);
			var parsedShader = shader.TryParse(out var shaderInfo);
			Assert.IsTrue(parsedShader, "Could not find or parse shader: " + shaderName);
			Assert.NotNull(shaderInfo);
			shaderInfo.AssertDefaults();

			var field = shaderInfo.Fields.FirstOrDefault(f => f.FieldName == "testField");
			Assert.NotNull(field, "Could not find shader field to be tested");
			if (field.TryFindUsage(shaderInfo.Kernels.First(), out var usage))
			{
				if(!expectFindingUsage) 
					Assert.Fail("Found usage of " + field.FieldName);
			}
			else
			{
				if (expectFindingUsage)
					Assert.Fail("Could not find usage of field in kernel");
			}
			return usage;
		}

		[Test]
		public void WriteInKernel()
		{
			var usage = BaseTest("FieldUsage_WriteInKernel");
			Assert.AreEqual(UsageType.Write, usage);
		}

		[Test]
		public void WriteInKernel_MultipleInOneLine()
		{
			var usage = BaseTest("FieldUsage_WriteInKernel_Multiple");
			Assert.AreEqual(UsageType.Write, usage);
		}

		[Test]
		public void ReadInKernel()
		{
			var usage = BaseTest("FieldUsage_ReadInKernel");
			Assert.AreEqual(UsageType.Read, usage);
		}

		[Test]
		public void ReadInKernel_LineComment()
		{
			var usage = BaseTest("FieldUsage_ReadInKernel_LineComment");
			Assert.AreEqual(UsageType.Unknown, usage);
		}

		[Test]
		public void ReadInKernel_LineComment2()
		{
			var usage = BaseTest("FieldUsage_ReadInKernel_LineComment2");
			Assert.AreEqual(UsageType.Unknown, usage);
		}

		[Test]
		public void ReadWriteInKernel()
		{
			var usage = BaseTest("FieldUsage_ReadWriteInKernel");
			Assert.AreEqual(UsageType.Write | UsageType.Read, usage);
		}
	}
}
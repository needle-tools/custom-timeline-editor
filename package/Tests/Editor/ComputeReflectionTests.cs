using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Needle.Timeline.Tests
{
	public static class ComputeReflectionTests
	{
		// TODO: add tests with cginc
		// TODO: add tests with multi_compile
		
		private static ComputeShader LoadShader(string name)
		{
			var shader = Resources.Load<ComputeShader>(name);
			Assert.NotNull(shader);
			return shader;
		}

		[Test]
		public static void Basic_1()
		{
			var shader = LoadShader("ComputeShaderTest01");
			shader.TryParse(out var shaderInfo);

			Debug.Log(shaderInfo);

			Assert.NotNull(shaderInfo);
			shaderInfo.AssertDefaults();
			Assert.AreEqual(shaderInfo.Kernels.Count, 1);
			Assert.AreEqual(shaderInfo.Fields.Count, 1);
			Assert.AreEqual(shaderInfo.Structs.Count, 0);
		}

		[Test]
		public static void Basic_2()
		{
			var shader = LoadShader("ComputeShaderTest02");
			shader.TryParse(out var shaderInfo);

			Debug.Log(shaderInfo);

			Assert.NotNull(shaderInfo);
			shaderInfo.AssertDefaults();
			Assert.AreEqual(shaderInfo.Kernels.Count, 1);
			Assert.AreEqual(shaderInfo.Fields.Count, 7);
			Assert.AreEqual(shaderInfo.Structs.Count, 1);
		}

		[Test]
		public static void Basic_3_UsingFields()
		{
			var shader = LoadShader("ComputeShader_Kernel_UsingFields_1");
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
		public static void Basic_3_UsingFields2()
		{
			var shader = LoadShader("ComputeShader_Kernel_UsingFields_2");
			shader.TryParse(out var shaderInfo);

			Debug.Log(shaderInfo);

			Assert.NotNull(shaderInfo);
			shaderInfo.AssertDefaults();
			Assert.IsNull(shaderInfo.Fields[0].Kernels, "Is not actually used: " + shaderInfo.Fields[0].FieldName);
			Assert.NotNull(shaderInfo.Fields[1].Kernels, "Failed finding kernel for " + shaderInfo.Fields[1]);
			Assert.IsTrue(shaderInfo.Fields[1].Kernels![0].Name == "CSMain");
		}

		[Test]
		public static void FindFieldsTest_MultipleKernels()
		{
			var shader = LoadShader("ComputeShader_MultipleKernels");
			shader.TryParse(out var shaderInfo);

			Debug.Log(shaderInfo);

			Assert.NotNull(shaderInfo);
			shaderInfo.AssertDefaults();
			Assert.AreEqual(shaderInfo.Kernels.Count, 2);
			Assert.AreEqual(shaderInfo.Fields.Count, 0);
			Assert.AreEqual(shaderInfo.Structs.Count, 0);
			Assert.AreEqual(shaderInfo.Kernels[0].Threads, new Vector3Int(8, 1, 128), shaderInfo.Kernels[0].ToString());
			Assert.AreEqual(shaderInfo.Kernels[1].Threads, new Vector3Int(1, 32, 32), shaderInfo.Kernels[1].ToString());
		}

		private static void AssertDefaults(this ComputeShaderInfo shaderInfo)
		{
			Assert.Greater(shaderInfo.Kernels.Count, 0);
			foreach(var k in shaderInfo.Kernels) 
				k.AssertDefaults();

			shaderInfo.Fields.AssertDefaults();
			foreach (var str in shaderInfo.Structs)
			{
				Assert.NotNull(str.Name, "struct is missing name");
				str.Fields.AssertDefaults();
			}
		}

		private static void AssertDefaults(this ComputeShaderKernelInfo info)
		{
			Assert.NotNull(info.Name);
			Assert.AreNotEqual(info.Threads, Vector3Int.zero, info.Name);
		}

		private static void AssertDefaults(this IEnumerable<ComputeShaderFieldInfo> info)
		{
			foreach (var i in info) i.AssertDefaults();
		}

		private static void AssertDefaults(this ComputeShaderFieldInfo info)
		{
			Assert.NotNull(info.TypeName, "missing type name");
			Assert.NotNull(info.FieldName, "missing field name");
			Assert.NotNull(info.FieldType, "missing field type: " + info.FieldName + ", " + info.TypeName);
			Assert.NotNull(info.FilePath, "missing file path");
		}
	}
}
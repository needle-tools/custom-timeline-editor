using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor.VersionControl;
using UnityEngine;

namespace Needle.Timeline.Tests
{
	public static class ComputeReflectionTests
	{
		// TODO: add tests with cginc
		// TODO: add tests with multi_compile
		

		[Test]
		public static void SimpleShader()
		{
			var shader = LoadShader("Simple");
			shader.TryParse(out var shaderInfo);

			Debug.Log(shaderInfo);

			Assert.NotNull(shaderInfo);
			shaderInfo.AssertDefaults();
			Assert.AreEqual( 1, shaderInfo.Kernels.Count);
			Assert.AreEqual(1, shaderInfo.Fields.Count);
			Assert.AreEqual(0, shaderInfo.Structs.Count);
		}

		[Test]
		public static void ShaderWithFieldsAndStruct()
		{
			var shader = LoadShader("ShaderWithFieldsAndStruct");
			shader.TryParse(out var shaderInfo);

			Debug.Log(shaderInfo);

			Assert.NotNull(shaderInfo);
			shaderInfo.AssertDefaults();
			Assert.AreEqual( 1, shaderInfo.Kernels.Count);
			Assert.AreEqual(7, shaderInfo.Fields.Count);
			Assert.AreEqual(1, shaderInfo.Structs.Count);
			Assert.AreEqual(12, shaderInfo.Structs[0].CalcStride());
			Assert.AreEqual(2, shaderInfo.Structs[0].Fields.Count);
		}

		[Test]
		public static void FieldUsedInKernel()
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
		public static void FieldNotUsedInKernel()
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

		[Test]
		public static void ShaderWithTwoKernels()
		{
			var shader = LoadShader("ShaderWithTwoKernels");
			shader.TryParse(out var shaderInfo);

			Debug.Log(shaderInfo);

			Assert.NotNull(shaderInfo);
			shaderInfo.AssertDefaults();
			Assert.AreEqual(2, shaderInfo.Kernels.Count);
			Assert.AreEqual(0, shaderInfo.Fields.Count);
			Assert.AreEqual(0, shaderInfo.Structs.Count);
			Assert.AreEqual(new Vector3Int(1, 1, 128), shaderInfo.Kernels[0].Threads, shaderInfo.Kernels[0].ToString());
			Assert.AreEqual(new Vector3Int(8, 24, 32), shaderInfo.Kernels[1].Threads, shaderInfo.Kernels[1].ToString());
		}
		
		[Test]
		public static void BufferWithStruct()
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
		
		
		
		
		
		[Test]
		public static void SetValues_BufferWithFloat()
		{
			var shader = LoadShader("SetValues/BufferWithFloat");
			shader.TryParse(out var shaderInfo);
			Assert.NotNull(shaderInfo);

			var list = new List<ComputeShaderBinding>();
			var source = new BufferWithFloatType();
			source.MyBuffer.Add(42);
			shaderInfo.Bind(source, new ResourceProvider(DefaultResources.GlobalComputeBufferProvider), list);
			Assert.AreEqual(1, list.Count);
			
			list[0].SetValue();
			shaderInfo.Shader.Dispatch(0, 1, 1, 1);
			var val = list[0].GetValue() as Array;
			Assert.NotNull(val);
			Assert.AreEqual(43,val.GetValue(0));
		}

		private class BufferWithFloatType
		{
			public List<float> MyBuffer = new List<float>();
		}


		private static ComputeShader LoadShader(string name)
		{
			var shader = Resources.Load<ComputeShader>(name);
			Assert.NotNull(shader);
			return shader;
		}

		private static void AssertDefaults(this ComputeShaderInfo shaderInfo)
		{
			Assert.NotNull(shaderInfo.Shader);
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
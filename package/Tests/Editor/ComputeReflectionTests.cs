using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor.MemoryProfiler;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

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
		public static void FieldUsedInKernel_2()
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
			Assert.AreEqual(0, shaderInfo.Kernels![0].Index);
			Assert.AreEqual(1, shaderInfo.Kernels![1].Index);
		}

		[Test]
		public static void FindTextures()
		{
			var shader = LoadShader("FindTextures2D");
			shader.TryParse(out var shaderInfo);

			Debug.Log(shaderInfo);

			Assert.NotNull(shaderInfo);
			shaderInfo.AssertDefaults();
			Assert.AreEqual(1, shaderInfo.Kernels.Count);
			Assert.AreEqual(3, shaderInfo.Fields.Count);
			Assert.AreEqual(0, shaderInfo.Structs.Count);
			Assert.AreEqual(typeof(Texture2D), shaderInfo.Fields[0].FieldType);
			Assert.AreEqual(typeof(Texture2D), shaderInfo.Fields[1].FieldType);
			Assert.AreEqual(typeof(Texture2D), shaderInfo.Fields[2].FieldType);
			
			Assert.AreEqual(false, shaderInfo.Fields[0].RandomWrite);
			Assert.AreEqual(true, shaderInfo.Fields[2].RandomWrite);
			
			Assert.AreEqual(4, shaderInfo.Fields[0].Stride);
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




		private static IResourceProvider TestsResourceProvider => new ResourceProvider(new DefaultComputeBufferProvider(), new DefaultRenderTextureProvider());
		
		private class TypeWithSomeFields
		{
			int myInt;
			float myFloat;
			uint myUint;
			Vector2Int myVector2Int;
			Vector2 myVector2;
			Vector3 myVector3;
			Vector4 myVector4;
			Matrix4x4 myMatrix;
		}
		
		[Test]
		public static void Auto_Bind_TypeWithSomeFields()
		{
			var shader = LoadShader("SetValues/SomeFields");
			shader.TryParse(out var shaderInfo);
			Assert.NotNull(shaderInfo);
			Assert.AreEqual(8, shaderInfo.Fields.Count, "Did not find all fields in shader");
			
			var list = new List<ComputeShaderBinding>();
			shaderInfo.Bind(typeof(TypeWithSomeFields), list, TestsResourceProvider);
			Assert.AreEqual(8, list.Count, "Failed binding all fields to shader fields");

			shaderInfo.Dispatch(new TypeWithSomeFields(), 0, list);
		}

		private class BufferWithFloatType
		{
			// ReSharper disable once CollectionNeverQueried.Local
			public readonly List<float> MyBuffer = new List<float>();
		}
		
		[Test]
		public static void Auto_Bind_FloatBuffer()
		{
			var shader = LoadShader("SetValues/BufferWithFloat");
			shader.TryParse(out var shaderInfo);
			Assert.NotNull(shaderInfo);
			
			var list = new List<ComputeShaderBinding>();
			shaderInfo.Bind(typeof(BufferWithFloatType), list, TestsResourceProvider);
			
			Assert.AreEqual(1, list.Count);
		}
		
		[Test]
		public static void Auto_Bind_FloatBuffer_AndDispatch()
		{
			var shader = LoadShader("SetValues/BufferWithFloat");
			shader.TryParse(out var shaderInfo);
			Assert.NotNull(shaderInfo);

			var source = new BufferWithFloatType();
			source.MyBuffer.Add(42);
			
			var list = new List<ComputeShaderBinding>();
			shaderInfo.Bind(typeof(BufferWithFloatType), list, TestsResourceProvider);
			Assert.AreEqual(1, list.Count);

			shaderInfo.Dispatch(source, 0, list);
			var val = list[0].GetValue() as Array;
			Assert.NotNull(val);
			Assert.AreEqual(43,val.GetValue(0));
		}

		private class MappingType
		{
			[ShaderField("MyBuffer")]
			public readonly List<float> MyList = new List<float>();
		}
		
		[Test]
		public static void Auto_ReMapBoundField()
		{
			var shader = LoadShader("SetValues/BufferWithFloat");
			shader.TryParse(out var shaderInfo);
			Assert.NotNull(shaderInfo);

			var list = new List<ComputeShaderBinding>();
			shaderInfo.Bind(typeof(MappingType), list, TestsResourceProvider);
			Assert.AreEqual(1, list.Count);
			Assert.IsTrue(list[0].TypeField.Name == nameof(MappingType.MyList));
			Assert.IsTrue(list[0].ShaderField.FieldName == "MyBuffer");
		}

		private class TestBindTexture2D
		{
			[TextureInfo(1024, 1024, TextureFormat = TextureFormat.RFloat)]
			public RenderTexture MyTexture;
		}
		
		[Test]
		public static void Auto_Bind_Texture2D()
		{
			var shader = LoadShader("SetValues/Bind_Texture2D");
			shader.TryParse(out var shaderInfo);
			Assert.NotNull(shaderInfo);
			
			var list = new List<ComputeShaderBinding>();
			shaderInfo.Bind(typeof(TestBindTexture2D), list, TestsResourceProvider);
			
			Assert.AreEqual(1, list.Count);
			var instance = new TestBindTexture2D();
			shaderInfo.Dispatch(instance, 0, list);
			Assert.NotNull(instance.MyTexture);
			Assert.AreEqual(GraphicsFormat.R32_SFloat, instance.MyTexture.graphicsFormat);
			Assert.AreEqual(1024, instance.MyTexture.width);
			Assert.AreEqual(1024, instance.MyTexture.height);
		}

		
		
		// UTILS
		// ---------------------
		
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
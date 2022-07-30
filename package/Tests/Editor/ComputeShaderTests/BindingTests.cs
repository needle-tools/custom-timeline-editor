using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Needle.Timeline.Tests.Bind_ComputeShaderTests
{
	public class BindingTests : ShaderTestsBase
	{
		
		
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
			Vector2 start;
		}
		
		[Test]
		public void Bind_BasicTypes()
		{
			var shader = LoadShader("BindBasicValues");
			shader.TryParse(out var shaderInfo);
			Assert.NotNull(shaderInfo);
			Assert.AreEqual(9, shaderInfo.Fields.Count, "Did not find all fields in shader");

			foreach (var f in shaderInfo.Fields)
			{
				Assert.NotNull(f.Kernels, "Didnt find kernel usage for " + f.FieldName);
			}
			
			var list = new List<ComputeShaderBinding>();
			shaderInfo.Bind(typeof(TypeWithSomeFields), list, TestsResourceProvider);
			Assert.AreEqual(9, list.Count, "Failed binding all fields to shader fields");

			shaderInfo.Dispatch(new TypeWithSomeFields(), 0, list);
		}
		
		

		private class BufferWithFloatType
		{
			// ReSharper disable once CollectionNeverQueried.Local
			public readonly List<float> MyBuffer = new List<float>();
		}
		
		[Test]
		public void Bind_FloatBuffer()
		{
			var shader = LoadShader("SetValues/BufferWithFloat");
			shader.TryParse(out var shaderInfo);
			Assert.NotNull(shaderInfo);
			
			var list = new List<ComputeShaderBinding>();
			shaderInfo.Bind(typeof(BufferWithFloatType), list, TestsResourceProvider);
			
			Assert.AreEqual(1, list.Count);
		}

		
		[Test]
		public void Bind_FloatBuffer_AndDispatch()
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
			var val = list[0].GetValue(source) as Array;
			Assert.NotNull(val);
			Assert.AreEqual(43,val.GetValue(0));
		}
		
		
		
		private class TypeWithComputeBufferField
		{
			[ComputeBufferInfo(Size = 1, Stride = sizeof(float))]
			public ComputeBuffer MyBuffer;
		}
		
		[Test]
		public void Bind_ComputeBuffer_AndDispatch()
		{
			var shader = LoadShader("SetValues/Bind_ComputeBuffer");
			shader.TryParse(out var shaderInfo);
			Assert.NotNull(shaderInfo);

			var source = new TypeWithComputeBufferField();
			
			var list = new List<ComputeShaderBinding>();
			shaderInfo.Bind(typeof(TypeWithComputeBufferField), list, TestsResourceProvider);
			Assert.AreEqual(1, list.Count, "Did not find field");

			shaderInfo.Dispatch(source, 0, list);
			Assert.NotNull(source.MyBuffer);
			Assert.IsTrue(source.MyBuffer.IsValid(), "Buffer is not valid");
			Assert.AreEqual(1, source.MyBuffer.count);
			Assert.AreEqual(sizeof(float),source.MyBuffer.stride);

			var res = new float[1];
			source.MyBuffer.GetData(res);
			Assert.AreEqual(111,res[0]);
		}


	}
}
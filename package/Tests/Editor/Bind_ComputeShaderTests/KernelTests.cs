using System;
using System.Collections.Generic;
using Needle.Timeline.ResourceProviders;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Needle.Timeline.Tests.Bind_ComputeShaderTests
{
	internal class Kernels : ShaderTestsBase
	{
		// TODO: cleanup tests
		// TODO: add tests with cginc
		// TODO: add tests with multi_compile
		
		[Test]
		public void FindKernels()
		{
			var shader = LoadShader("FindKernels");
			shader.TryParse(out var shaderInfo);
			Assert.NotNull(shaderInfo);
			Assert.AreEqual(2, shaderInfo.Kernels.Count);
			Assert.AreEqual(new Vector3Int(1,1,1), shaderInfo.Kernels[0].Threads);
			Assert.AreEqual(new Vector3Int(1,1,1), shaderInfo.Kernels[1].Threads);
		}
		

		[Test]
		public void FindOneKernelAndOneField()
		{
			var shader = LoadShader("ShaderWithOneField");
			shader.TryParse(out var shaderInfo);

			Debug.Log(shaderInfo);

			Assert.NotNull(shaderInfo);
			shaderInfo.AssertDefaults();
			Assert.AreEqual( 1, shaderInfo.Kernels.Count);
			Assert.AreEqual(1, shaderInfo.Fields.Count);
			Assert.AreEqual(0, shaderInfo.Structs.Count);
		}
		
		[Test]
		public void ShaderWithTwoKernels()
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

	}
}
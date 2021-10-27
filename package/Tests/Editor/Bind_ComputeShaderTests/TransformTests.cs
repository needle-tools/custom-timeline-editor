using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Needle.Timeline.Tests.Bind_ComputeShaderTests
{
	public class TransformTests : ShaderTestsBase
	{
		private class Bind_TransformType
		{
			public Transform Point3;
			public Transform Point4;
			public Transform Matrix;
		}

		[Test]
		public void Bind_Transform()
		{
			var shader = LoadShader("TransformToValues");
			shader.TryParse(out var shaderInfo);
			Debug.Log(shaderInfo);
			Assert.NotNull(shaderInfo);
			
			var list = new List<ComputeShaderBinding>();
			shaderInfo.Bind(typeof(Bind_TransformType), list, TestsResourceProvider);
			
			Assert.AreEqual(3, list.Count);
			Assert.AreEqual("float3", list[0].ShaderField.TypeName);
			Assert.AreEqual("float4", list[1].ShaderField.TypeName);
			Assert.AreEqual("float4x4", list[2].ShaderField.TypeName);
		}
	}
}
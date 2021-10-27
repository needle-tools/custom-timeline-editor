using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Needle.Timeline.Tests.Bind_ComputeShaderTests
{
	public class TextureTests : ShaderTestsBase
	{

		[Test]
		public void FindTextures()
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
			Assert.AreEqual("float", shaderInfo.Fields[0].GenericTypeName);
			Assert.AreEqual("float2", shaderInfo.Fields[1].GenericTypeName);
			Assert.AreEqual("float4", shaderInfo.Fields[2].GenericTypeName);
			
			Assert.AreEqual(false, shaderInfo.Fields[0].RandomWrite);
			Assert.AreEqual(true, shaderInfo.Fields[2].RandomWrite);
			
			Assert.AreEqual(4, shaderInfo.Fields[0].Stride);
		}

		private class TestBindTexture2D
		{
			[TextureInfo(1024, 1024, TextureFormat = TextureFormat.RFloat)]
			public RenderTexture MyTexture; 
		}
		
		[Test]
		public void BindWithAttribute()
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

	}
}
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Needle.Timeline.Tests.BridgeTests
{
	internal class TextureInfoTests
	{
		[Test]
		public void InvalidTextureInfo_Produces_InvalidRenderTextureInfo()
		{
			var info = new TextureInfo().ToRenderTextureDescription();
			
			Assert.IsFalse(info.Validate(), "RTDesc is valid");
		}
		
		[Test]
		public void ValidTextureInfo_Produces_ValidRenderTextureInfo()
		{
			var info = new TextureInfo(100, 100).ToRenderTextureDescription();
			
			Assert.IsTrue(info.Validate(), "RTDesc is not valid");
			Assert.AreEqual(100, info.Width);
			Assert.AreEqual(100, info.Height);
		}
	}
}
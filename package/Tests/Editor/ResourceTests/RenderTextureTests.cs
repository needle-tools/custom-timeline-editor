using Needle.Timeline.ResourceProviders;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Needle.Timeline.Tests.ResourceTests
{
	public class RenderTextureTests : ResourceTestBase
	{
		[Test]
		public void CreateRenderTexture()
		{
			var res = Resources;

			var tex = res.RenderTextureProvider.GetTexture("test", new RenderTextureDescription()
			{
				Width = 100, Height = 100, Format = RenderTextureFormat.ARGB32
			});

			Assert.NotNull(tex);
			Assert.AreEqual(100, tex.width);
			Assert.AreEqual(100, tex.height);
			Assert.AreEqual(RenderTextureFormat.ARGB32, tex.format);
			Assert.IsFalse(tex.enableRandomWrite, "Texture has random write enabled");
		}
		
		[Test]
		public void CreateRandomWriteRenderTexture()
		{
			var res = Resources;

			var tex = res.RenderTextureProvider.GetTexture("test", new RenderTextureDescription()
			{
				Width = 100, Height = 100, RandomAccess = true
			});

			Assert.NotNull(tex);
			Assert.IsTrue(tex.enableRandomWrite, "Texture has not random write enabled");
		}
		
		[Test]
		public void RenderTextureDescriptions_AreEqual()
		{
			var desc1 = new RenderTextureDescription()
			{
				Width = 100, Height = 100
			};
			var desc2 = new RenderTextureDescription()
			{
				Width = 100, Height = 100
			};
			
			Assert.IsTrue(desc1.Equals(desc2));
		}
		
		[Test]
		public void RenderTextureDescription_Equals_RenderTexture()
		{
			var res = Resources;
			var desc = new RenderTextureDescription()
			{
				Width = 100, Height = 100
			};
			var tex = res.RenderTextureProvider.GetTexture("test", desc);

			var result = desc.Equals(tex);

			Assert.IsTrue(result);
		}
		
		[Test]
		public void GetTextureWithSameId_IsSame()
		{
			var res = Resources;

			var tex1 = res.RenderTextureProvider.GetTexture("test", new RenderTextureDescription()
			{
				Width = 100, Height = 100
			});
			var tex2 = res.RenderTextureProvider.GetTexture("test", new RenderTextureDescription()
			{
				Width = 100, Height = 100
			});

			Assert.NotNull(tex1);
			Assert.NotNull(tex2);
			Assert.IsTrue(tex1 == tex2, "Resource provider returned different instances for same id");
		}
	}
}
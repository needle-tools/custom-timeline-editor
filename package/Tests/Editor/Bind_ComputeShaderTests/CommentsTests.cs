using NUnit.Framework;
using UnityEngine;

namespace Needle.Timeline.Tests.Bind_ComputeShaderTests
{
	public class CommentsTests : ShaderTestsBase
	{
		[Test]
		public void IgnoreCommentedFields()
		{
			var shader = LoadShader("SkipComments");
			shader.TryParse(out var shaderInfo);
			Debug.Log(shaderInfo);
			Assert.NotNull(shaderInfo);
			shaderInfo.AssertDefaults();
			Assert.AreEqual(0, shaderInfo.Fields.Count);
		}
	}
}
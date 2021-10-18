using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Needle.Timeline.Tests
{
	public class ComputeReflectionTests
	{
		private ComputeShader LoadShader(string name)
		{
			var shader = Resources.Load<ComputeShader>(name);
			Assert.NotNull(shader);
			return shader;
		}

		[Test]
		public void FindFieldsTest_01()
		{
			var shader = LoadShader("ComputeShaderTest01");
			var list = new List<ComputeShaderFieldInfo>();
			shader.FindFields(list);
			Assert.IsNotEmpty(list);
			// var expected = new ComputeShaderFieldInfo(shader, null, 0,)
		}
	}
}
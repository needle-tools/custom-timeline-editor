using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Needle.Timeline.Tests
{
	internal static class ShaderTestExtensions
	{
		internal static void AssertDefaults(this ComputeShaderInfo shaderInfo)
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

		internal static void AssertDefaults(this ComputeShaderKernelInfo info)
		{
			Assert.NotNull(info.Name);
			Assert.AreNotEqual(info.Threads, Vector3Int.zero, info.Name);
		}

		internal static void AssertDefaults(this IEnumerable<ComputeShaderFieldInfo> info)
		{
			foreach (var i in info) i.AssertDefaults();
		}

		internal static void AssertDefaults(this ComputeShaderFieldInfo info)
		{
			Assert.NotNull(info.TypeName, "missing type name");
			Assert.NotNull(info.FieldName, "missing field name");
			Assert.NotNull(info.FieldType, "missing field type: " + info.FieldName + ", " + info.TypeName);
			Assert.NotNull(info.FilePath, "missing file path");
		}
	}
}
using NUnit.Framework;
using UnityEngine;

namespace Needle.Timeline.Tests
{
	public class StrideTests
	{
		
		[Test]
		public void Struct_Int()
		{
			Assert.IsTrue(typeof(Stride0_).GetStride() == 4);
		}
		struct Stride0_
		{
			int v0;
		}
		
		[Test]
		public void Struct_Int_Int()
		{
			Assert.IsTrue(typeof(Stride1_).GetStride() == 8);
		}
		struct Stride1_
		{
			private int v0, v1;
		}
		
		[Test]
		public void Struct_Int_Int_Float()
		{
			Assert.IsTrue(typeof(Stride2_).GetStride() == 12);
		}
		struct Stride2_
		{
			private int v0, v1;
			private float v2;
		}
		
		[Test]
		public void Struct_Float_Vector2()
		{
			Assert.IsTrue(typeof(Stride3_).GetStride() == sizeof(float)*3);
		}
		struct Stride3_
		{
			private float v0;
			private Vector2 v1;
		}
		
		[Test]
		public void Struct_Struct()
		{
			Assert.IsTrue(typeof(Stride4_).GetStride() == typeof(Stride3_).GetStride());
		}
		struct Stride4_
		{
			private Stride3_ v0;
		}
	}
}
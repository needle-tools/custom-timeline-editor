using NUnit.Framework;
using UnityEngine;

namespace Needle.Timeline.Tests
{
	public class BufferInterpolateTests
	{
		private ComputeShader LoadShader()
		{
			var shader = Resources.Load<ComputeShader>("Timeline/Interpolate");
			Assert.NotNull(shader);
			return shader;
		}
		
		[Test]
		public void Interpolate_Int()
		{
			var shader = LoadShader();
			Assert.IsTrue(TestInterpolate<int>(shader, "INT", sizeof(int), 0, 100) == 50);
			Assert.IsTrue(TestInterpolate<int>(shader, "INT", sizeof(int), 0, -100) == -50);
		}

		[Test]
		public void Interpolate_Float()
		{
			var shader = LoadShader();
			Assert.IsTrue(Mathf.Approximately(TestInterpolate<float>(shader, "FLOAT", sizeof(float), 0, 1), 
				.5f));
			Assert.IsTrue(Mathf.Approximately(TestInterpolate<float>(shader, "FLOAT", sizeof(float), 0, 1, 1), 
				1));
		}

		[Test]
		public void Interpolate_Float2()
		{
			var shader = LoadShader();
			Assert.IsTrue(TestInterpolate(shader, "FLOAT2", sizeof(float) * 2, new Vector2(), Vector2.one) == 
			              new Vector2(.5f, .5f));
		}

		[Test]
		public void Interpolate_Float3()
		{
			var shader = LoadShader();
			Assert.IsTrue(TestInterpolate(shader, "FLOAT3", sizeof(float) * 3, new Vector3(), Vector3.one) == 
			              new Vector3(.5f, .5f, .5f));
		}

		[Test]
		public void Interpolate_Float4()
		{
			var shader = LoadShader();
			Assert.IsTrue(TestInterpolate(shader, "FLOAT4", sizeof(float) * 4, new Vector4(), Vector4.one) == 
			              new Vector4(.5f, .5f, .5f, .5f));
			Assert.IsTrue(TestInterpolate(shader, "FLOAT4", sizeof(float) * 4, new Vector4(), new Vector4(100,1,0,.5f)) == 
			              new Vector4(50, .5f, 0, .25f));
		}

		[Test]
		public void Interpolate_Custom1()
		{
			var shader = LoadShader();
			var res = TestInterpolate(shader, "FLOAT2", sizeof(float)*2, new CustomType1(), new CustomType1(){v0=1, v1=1});
			Assert.IsTrue(Mathf.Approximately(res.v0, 0.5f));
			Assert.IsTrue(Mathf.Approximately(res.v1, 0.5f));
		}
		private struct CustomType1
		{
			public float v0;
			public float v1;
			public override string ToString()
			{
				return "v0=" + v0 + ", v1=" + v1;
			}
		}

		[Test]
		public void Interpolate_Custom2()
		{
			var shader = LoadShader();
			var res = TestInterpolate(shader, "FLOAT3", sizeof(float)*3, new CustomType2(), new CustomType2(){v0=1, v1=new Vector2(1,1)});
			Assert.IsTrue(Mathf.Approximately(res.v0, 0.5f));
			Assert.IsTrue(res.v1 == new Vector2(.5f,.5f));
		}
		private struct CustomType2
		{
			public float v0;
			public Vector2 v1;
			public override string ToString()
			{
				return "v0=" + v0 + ", v1=" + v1;
			}
		}

		
		
		
		
		
		
		
		private static T TestInterpolate<T>(ComputeShader shader, string keyword, int stride, T t0, T t1, float t = 0.5f) where T : struct
		{
			using var i0 = new ComputeBuffer(1, stride);
			i0.SetData(new[]{t0});
			using var i1 = new ComputeBuffer(1, stride);
			i1.SetData(new[]{t1});
			using var res = new ComputeBuffer(1, stride, ComputeBufferType.Structured);
			shader.EnableKeyword(keyword);
			shader.SetBuffer(0, "i0", i0);
			shader.SetBuffer(0, "i1", i1);
			shader.SetBuffer(0, "res", res);
			shader.SetFloat("t", t);
			shader.Dispatch(0, 1, 1, 1);
			shader.DisableKeyword(keyword);
			var data = new T[1];
			res.GetData(data);
			var output = data[0];
			Debug.Log(t0 + " -> " + t1 + " = " + output);
			return output;
		}
		
		
		
	}
}
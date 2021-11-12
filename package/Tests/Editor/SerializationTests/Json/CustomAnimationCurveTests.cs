using System.Collections.Generic;
using System.Linq;
using Needle.Timeline.CurveEasing;
using Needle.Timeline.Serialization;
using NUnit.Framework;
using UnityEngine;

namespace Needle.Timeline.Tests.SerializationTests.Json
{
	public class CustomAnimationCurveTests
	{
		[Test]
		public void SerializeCurve_ProducesExpectedJson()
		{
			var curve = new CustomAnimationCurve<List<int>>();
			curve.DefaultEasing = new NoEasing();
			var ser = new NewtonsoftSerializer() { Indented = false };

			var json = (string)ser.Serialize(curve);

			Assert.IsFalse(string.IsNullOrEmpty(json), "No json?");
			Assert.AreEqual(AnimationCurveJsonData.AnimationCurve0, json);
		}
		
		[Test]
		public void MultipleSerializationsOfSameObject_ProduceSameJson()
		{
			var curve = new CustomAnimationCurve<List<int>>();
			curve.DefaultEasing = new NoEasing();
			var ser = new NewtonsoftSerializer() { Indented = false };

			var json = (string)ser.Serialize(curve);
			var json2 = (string)ser.Serialize(curve);

			Assert.IsFalse(string.IsNullOrEmpty(json), "No json?");
			Assert.IsFalse(string.IsNullOrEmpty(json2), "No json?");
			Assert.AreEqual(json, json2, "Json changed");
		}
		
		[Test]
		public void ReadCurveJson_ProducesCorrectCurve()
		{
			var ser = new NewtonsoftSerializer() { Indented = false };

			var curve = ser.Deserialize<CustomAnimationCurve<List<int>>>(AnimationCurveJsonData.AnimationCurve0);

			Assert.IsNotNull(curve);
			Assert.NotNull(curve.Keyframes, "Keyframes are null");
			Assert.AreEqual(0, curve.Keyframes.Count);
			Assert.IsTrue(curve.SupportedTypes.Contains(typeof(List<int>)));
			Assert.NotNull(curve.DefaultEasing, "Default easing is null");
			Assert.AreEqual(typeof(NoEasing),curve.DefaultEasing.GetType());
		}
		
		[Test]
		public void SerializeAndDeserialize_ProducesSameCurve()
		{
			var curve = new CustomAnimationCurve<List<int>>();
			curve.DefaultEasing = new NoEasing();
			var ser = new NewtonsoftSerializer() { Indented = false };

			var json = (string)ser.Serialize(curve);
			var curve2 = ser.Deserialize<CustomAnimationCurve<List<int>>>(json);
			
			Assert.NotNull(curve2.Keyframes, "Keyframes are null");
			Assert.AreEqual(0, curve2.Keyframes.Count);
			Assert.IsTrue(curve2.SupportedTypes.Contains(typeof(List<int>)));
			Assert.NotNull(curve2.DefaultEasing, "Default easing is null");
			Assert.AreEqual(typeof(NoEasing),curve2.DefaultEasing.GetType());
		}
		
		[Test]
		public void ReadAndSerialize_ProducesOriginalJson()
		{
			var ser = new NewtonsoftSerializer() { Indented = false };
			var jsonString = AnimationCurveJsonData.AnimationCurve0;

			var curve = ser.Deserialize<CustomAnimationCurve<List<int>>>(jsonString);
			var newJsonString = (string)ser.Serialize(curve);
			
			Assert.AreEqual(jsonString, newJsonString, "Json changed");
		}
	}
}
using Needle.Timeline.Serialization;
using NUnit.Framework;
using UnityEngine;
// ReSharper disable ClassNeverInstantiated.Global

namespace Needle.Timeline.Tests.SerializationTests.Json
{
	public class RecoveryTests
	{
		[RefactorInfo("MyOldTypeName")]
		public class MyNewTypeName
		{
			
		}
		
		[Test]
		public void CanResolve_RenameType()
		{
			const string inputJson = "{\"$type\":\"Needle.Timeline.Tests.SerializationTests.Json.RecoveryTests+MyOldTypeName, Needle.Timeline-Tests\"}";
			
			var ser = new NewtonsoftSerializer() { Indented = false };
			
			var res = ser.Deserialize<MyNewTypeName>(inputJson);

			Assert.NotNull(res);
			Assert.IsAssignableFrom(typeof(MyNewTypeName), res);
		}
		
		[Test]
		public void CanResolve_RenameTypeAndNamespace()
		{
			const string inputJson = "{\"$type\":\"Needle.Timeline.Tests.SerializationTests.InOtherNamespace+MyOldTypeName, Needle.Timeline-Tests\"}";
			
			var ser = new NewtonsoftSerializer() { Indented = false };
			
			var res = ser.Deserialize<MyNewTypeName>(inputJson);

			Assert.NotNull(res);
			Assert.IsAssignableFrom(typeof(MyNewTypeName), res);
		}
		
		[Test]
		public void CanResolve_RenameNamespace()
		{
			const string inputJson = "{\"$type\":\"Needle.Timeline.Tests.SerializationTests.InOtherNamespace+MyNewTypeName, Needle.Timeline-Tests\"}";
			
			var ser = new NewtonsoftSerializer() { Indented = false };
			
			var res = ser.Deserialize<MyNewTypeName>(inputJson);

			Assert.NotNull(res);
			Assert.IsAssignableFrom(typeof(MyNewTypeName), res);
		}
	}
}
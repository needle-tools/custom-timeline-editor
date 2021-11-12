using Needle.Timeline.Serialization;
using NUnit.Framework;
using UnityEngine;

namespace Needle.Timeline.Tests.SerializationTests.Json
{
	public class RecoveryTests
	{
		public class MyNewTypeName
		{
			
		}
		
		[Test]
		public void SerializedType_Rename_IsFound()
		{
			const string inputJson = "{\"$type\":\"Needle.Timeline.Tests.SerializationTests.Json.RecoveryTests+MyOldTypeName, Needle.Timeline-Tests\"}";
			
			var ser = new NewtonsoftSerializer() { Indented = false };
			
			var res = ser.Deserialize<MyNewTypeName>(inputJson);

			Assert.NotNull(res, "TODO: make this work");
		}
	}
}
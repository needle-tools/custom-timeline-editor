using Needle.Timeline.Serialization;
using NUnit.Framework;
using UnityEngine;

namespace Needle.Timeline.Tests.SerializationTests.Json
{
	public class CallbackTests
	{
		public class ObjectWithCallback : ISerializationCallbackReceiver
		{
			public bool ReceivedBeforeSerialize;
			public bool ReceivedAfterDeserialize;
			
			public void OnBeforeSerialize()
			{
				ReceivedBeforeSerialize = true;
			}

			public void OnAfterDeserialize()
			{
				ReceivedAfterDeserialize = true;
			}
		}
		
		[Test]
		public void Receive_OnBeforeSerializeCallback()
		{
			var obj = new ObjectWithCallback();
			var ser = new NewtonsoftSerializer();

			ser.Serialize(obj);

			Assert.IsTrue(obj.ReceivedBeforeSerialize);
		}
		
		[Test]
		public void Receive_OnAfterDeserializeCallback()
		{
			var json = "{\"$type\":\"Needle.Timeline.Tests.SerializationTests.Json.CallbackTests+ObjectWithCallback, Needle.Timeline-Tests\"}";
			var ser = new NewtonsoftSerializer();

			var obj = ser.Deserialize<ObjectWithCallback>(json);

			Assert.NotNull(obj, "Could not deserialize");
			Assert.IsTrue(obj.ReceivedAfterDeserialize);
		}
		
		
		
	}
}
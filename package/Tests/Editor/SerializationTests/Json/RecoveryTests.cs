using Needle.Timeline.Serialization;
using NUnit.Framework;
using UnityEngine;
// ReSharper disable ClassNeverInstantiated.Global

namespace Needle.Timeline.Tests.SerializationTests.Json
{
	public class RecoveryTests
	{
		[RefactorInfo("MyOldTypeName")]
		public class TypeWasRenamed
		{
			
		}
		
		[Test]
		public void CanResolve_RenameType()
		{
			const string inputJson = "{\"$type\":\"Needle.Timeline.Tests.SerializationTests.Json.RecoveryTests+MyOldTypeName, Needle.Timeline-Tests\"}";
			
			var ser = new NewtonsoftSerializer() { Indented = false };
			
			var res = ser.Deserialize<TypeWasRenamed>(inputJson);

			Assert.NotNull(res);
			Assert.IsAssignableFrom(typeof(TypeWasRenamed), res);
		}
		
		[Test]
		public void CanResolve_RenameTypeAndNamespace()
		{
			const string inputJson = "{\"$type\":\"Needle.Timeline.Tests.SerializationTests.InOtherNamespace+MyOldTypeName, Needle.Timeline-Tests\"}";
			
			var ser = new NewtonsoftSerializer() { Indented = false };
			
			var res = ser.Deserialize<TypeWasRenamed>(inputJson);

			Assert.NotNull(res);
			Assert.IsAssignableFrom(typeof(TypeWasRenamed), res);
		}
		
		[RefactorInfo("Needle.Timeline.Tests.SerializationTests.InOtherNamespace+MyNewTypeName")]
		public class TypeMovedNamespace
		{
			
		}
		
		[Test]
		public void CanResolve_RenameNamespace()
		{
			const string inputJson = "{\"$type\":\"Needle.Timeline.Tests.SerializationTests.InOtherNamespace+MyNewTypeName, Needle.Timeline-Tests\"}";
			
			var ser = new NewtonsoftSerializer() { Indented = false };
			
			var res = ser.Deserialize<TypeMovedNamespace>(inputJson);

			Assert.NotNull(res);
			Assert.IsAssignableFrom(typeof(TypeMovedNamespace), res);
		}
		
		[Test]
		public void CanResolve_AssemblyChanged()
		{
			const string inputJson = "{\"$type\":\"Needle.Timeline.Tests.SerializationTests.Json.RecoveryTests+TypeMovedAssembly, Needle.Timeline-Tests\"}";
			
			var ser = new NewtonsoftSerializer() { Indented = false };
			
			var res = ser.Deserialize<TypeMovedAssembly>(inputJson);
			
			Assert.NotNull(res);
			Assert.IsAssignableFrom(typeof(TypeMovedAssembly), res);
		}
	}
}
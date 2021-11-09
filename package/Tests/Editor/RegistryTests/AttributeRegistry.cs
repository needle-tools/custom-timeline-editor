using System;
using NUnit.Framework;

namespace Needle.Timeline.Tests.RegistryTests
{
	internal class AttributeRegistryTests
	{
		public class Att0 : Attribute{}
		
		[Att0]
		public class TypeWithAtt0 {}
		
		[Test]
		public void FindTypeWithAttribute()
		{
			var reg = new AttributeRegistry<Att0>();

			var res = reg.TryFind(e => true, out var type);

			Assert.IsTrue(res);
			Assert.AreEqual(typeof(TypeWithAtt0), type);
		}
		
		public class Att1 : Attribute, IPriority
		{
			public int Priority { get; set; }
			public Att1(int prio) => Priority = prio;
		}
		
		[Att1(0)]
		public class Type1WithAtt1 {}
		
		[Att1(1)]
		public class Type2WithAtt1 {}
		
		[Test]
		public void FindTypeWithAttribute_MultipleImplementors()
		{
			var reg = new AttributeRegistry<Att1>();

			var res = reg.TryFind(e => true, out var type);
			var all = reg.GetAll();

			Assert.IsTrue(res);
			Assert.AreEqual(typeof(Type2WithAtt1), type);
			Assert.AreEqual(typeof(Type1WithAtt1), all[1]);
			Assert.AreEqual(typeof(Type2WithAtt1), all[0]);
		}
		
		[Test]
		public void FindSpecific()
		{
			var reg = new AttributeRegistry<Att1>();

			var res = reg.TryFind(e => typeof(Type1WithAtt1).IsAssignableFrom(e), out var type);

			Assert.IsTrue(res);
			Assert.AreEqual(typeof(Type1WithAtt1), type);
		}
		
		public class UnusedAtt : Attribute{}
		
		[Test]
		public void FindNone()
		{
			var reg = new AttributeRegistry<UnusedAtt>();

			var res = reg.TryFind(e => true, out var type);

			Assert.IsFalse(res);
			Assert.IsNull(type);
		}
	}
}
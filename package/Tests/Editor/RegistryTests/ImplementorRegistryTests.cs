using NUnit.Framework;

namespace Needle.Timeline.Tests.RegistryTests
{
	internal class ImplementorRegistryTests
	{
		public interface INotImplemented
		{
			
		}
		
		[Test]
		public void FindNone()
		{
			var reg = new ImplementorsRegistry<INotImplemented>();

			var res = reg.TryFind(e => true, out _);
			var created = reg.TryGetInstance<INotImplemented>(out var inst);

			Assert.IsFalse(res);
			Assert.IsFalse(created);
			Assert.IsNull(inst);
		}
		
		public interface IInterface0
		{
			
		}

		public class Impl0 : IInterface0{}
		
		[Test]
		public void FindOne()
		{
			var reg = new ImplementorsRegistry<IInterface0>();

			var res = reg.TryFind(e => true, out var type);
			var created = reg.TryGetInstance<Impl0>(out var inst);

			Assert.IsTrue(res);
			Assert.AreEqual(typeof(Impl0), type);
			Assert.IsTrue(created);
			Assert.NotNull(inst);
			Assert.AreEqual(typeof(Impl0), inst.GetType());
		}
	}
}
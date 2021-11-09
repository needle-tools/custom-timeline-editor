using NUnit.Framework;

namespace Needle.Timeline.Tests
{
	public class ViewValueProxyTests
	{
		[Test]
		public void GetValue()
		{
			var proxy = new ViewValueProxy(1);

			var val = proxy.GetValue();

			Assert.AreEqual(1, val);
		}
		
		[Test]
		public void SetValue()
		{
			var proxy = new ViewValueProxy(null);

			proxy.SetValue(100);

			Assert.AreEqual(100, proxy.GetValue());
		}
		
		[Test]
		public void RaiseChangeEvent()
		{
			var proxy = new ViewValueProxy(1);
			var changed = false;
			
			proxy.ValueChanged += newValue =>
			{
				changed = true;
				Assert.AreEqual(100, newValue);
			};
			proxy.SetValue(100);

			Assert.IsTrue(changed, "Received no change event");
		}
	}
}
using NUnit.Framework;
using UnityEditor.UIElements;

namespace Needle.Timeline.Tests
{
	internal class EnumBuilderTests
	{
		public enum MyEnum
		{
			Opt1,
			Opt2
		}
		
		[Test]
		public void CanBuild()
		{
			var builder = new EnumBuilder();
			Assert.IsTrue(builder.CanBuild(typeof(MyEnum)));
		}
		
		[Test]
		public void CreateBuilder()
		{
			var builder = new EnumBuilder();
			var handler = new ValueHandler(MyEnum.Opt1);

			var res = builder.Build(typeof(MyEnum), handler) as PopupField<string>;

			Assert.IsNotNull(res);
			Assert.AreEqual(MyEnum.Opt1.ToString(), res.value);
			Assert.AreEqual(MyEnum.Opt1, handler.GetValue());
		}
	}
}
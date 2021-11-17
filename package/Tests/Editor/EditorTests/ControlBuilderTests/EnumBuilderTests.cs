using NUnit.Framework;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

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
		public void BuildPopup()
		{
			var builder = new EnumBuilder();
			var handler = new ViewValueProxy("", MyEnum.Opt1);

			var res = builder.Build(typeof(MyEnum), handler, null) as PopupField<string>;

			Assert.IsNotNull(res);
			Assert.AreEqual(MyEnum.Opt1.ToString(), res.value);
			Assert.AreEqual(MyEnum.Opt1, handler.GetValue());
		}
		
		[Test]
		public void ViewChangesModel()
		{
			var builder = new EnumBuilder();
			var handler = new ViewValueProxy("", MyEnum.Opt1);

			var res = builder.Build(typeof(MyEnum), handler, null) as PopupField<string>;

			Assert.IsNotNull(res);
			Assert.AreEqual(MyEnum.Opt1.ToString(), res.value);
			Assert.AreEqual(MyEnum.Opt1, handler.GetValue());

			using var context = new InWindowContext();
			context.Add(res);
			res.value = (MyEnum.Opt2).ToString();
			Assert.AreEqual(MyEnum.Opt2.ToString(), res.value);
			Assert.AreEqual(MyEnum.Opt2, handler.GetValue());
		}
		
		[Test]
		public void ModelChangesView()
		{
			var builder = new EnumBuilder();
			var handler = new ViewValueProxy("", MyEnum.Opt1);
			var parent = new VisualElement();

			var res = builder.Build(typeof(MyEnum), handler, null) as PopupField<string>;
			parent.Add(res);

			Assert.IsNotNull(res);
			Assert.AreEqual(MyEnum.Opt1.ToString(), res.value);
			Assert.AreEqual(MyEnum.Opt1, handler.GetValue());

			handler.SetValue(MyEnum.Opt2);

			Assert.AreEqual(MyEnum.Opt2, handler.GetValue(), "Model didnt change");
			Assert.AreEqual(MyEnum.Opt2.ToString(), res.value, "View didnt change");
		}
	}
}
using System.Reflection;
using NUnit.Framework;

namespace Needle.Timeline.Tests
{
	internal class ViewFieldBindingTests
	{
		private class TypeWithFloat
		{
			public float Field;
		}
		
		[Test]
		public void BindingChangesModelValue() 
		{
			var instance = new TypeWithFloat();
			
			var field = instance.GetType().GetField("Field", BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public);
			Assert.NotNull(field);
			
			var binding = new FieldViewBinding(instance, FieldInfo.GetFieldFromHandle(field.FieldHandle));
			binding.SetValue(100);

			Assert.AreEqual(100, instance.Field);
		}
		
		[Test]
		public void ModelChangesBindingValue() 
		{
			var instance = new TypeWithFloat();
			
			var field = instance.GetType().GetField("Field", BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public);
			Assert.NotNull(field);
			
			var binding = new FieldViewBinding(instance, FieldInfo.GetFieldFromHandle(field.FieldHandle));
			instance.Field = 100;

			Assert.AreEqual(100, binding.GetValue());
		}
		
		[Test]
		public void ValueChange_RaisesChangeEvent() 
		{
			var instance = new TypeWithFloat();
			var changed = false;
			
			var field = instance.GetType().GetField("Field", BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public);
			Assert.NotNull(field);

			var binding = new FieldViewBinding(instance, FieldInfo.GetFieldFromHandle(field.FieldHandle));
			binding.ValueChanged += val =>
			{
				changed = true;
				Assert.AreEqual(100, val);
			};
			binding.SetValue(100);

			Assert.IsTrue(changed, "Changed event not raised");
		}
		
		[Test]
		public void ValueDoesntChange_DoesNotRaiseChangeEvent() 
		{
			var instance = new TypeWithFloat();
			instance.Field = 100;
			var changed = false;
			
			var field = instance.GetType().GetField("Field", BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public);
			Assert.NotNull(field);

			var binding = new FieldViewBinding(instance, FieldInfo.GetFieldFromHandle(field.FieldHandle));
			binding.ValueChanged += val =>
			{
				changed = true;
				Assert.AreEqual(100, val);
			};
			binding.SetValue((float)100);

			Assert.IsFalse(changed, "Change event raised without change");
		}
		
		[Test]
		public void ValueOfOtherTypeDoesntChange_DoesNotRaiseChangeEvent() 
		{
			var instance = new TypeWithFloat();
			instance.Field = 100;
			var changed = false;
			
			var field = instance.GetType().GetField("Field", BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public);
			Assert.NotNull(field);

			var binding = new FieldViewBinding(instance, FieldInfo.GetFieldFromHandle(field.FieldHandle));
			binding.ValueChanged += val =>
			{
				changed = true;
				Assert.AreEqual(100, val);
			};
			binding.SetValue((int)100);

			Assert.IsFalse(changed, "Change event raised without change");
		}
	}
}
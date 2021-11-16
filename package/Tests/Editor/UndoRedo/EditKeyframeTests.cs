using System.Collections.Generic;
using System.Windows.Input;
using Needle.Timeline.Commands;
using NUnit.Framework;
using UnityEditor.VersionControl;

namespace Needle.Timeline.Tests.UndoRedo
{
	public class EditKeyframeTests
	{
		[Test]
		public void Undo_ReferenceTypeValue_ProducesOriginal()
		{
			var kf = new CustomKeyframe<List<int>>();
			var original = kf.value = new List<int>(new int[] { 123 });

			ICommand undo = new EditKeyframeValue(kf) { IsDone = true };
			var changed = kf.value = new List<int>();
			
			undo.PerformUndo();
			
			Assert.IsTrue(kf.value != original, "Value should not match old value on undo");
			Assert.IsTrue(!ReferenceEquals(kf.value, original), "Reference should not match");
			Assert.AreEqual(1, kf.value.Count);
		}
		
		[Test]
		public void Undo_MutatingOriginalReference_DoesNotMutateUndoResult()
		{
			var kf = new CustomKeyframe<List<int>>();
			var original = kf.value = new List<int>(new[] { 123 });

			ICommand undo = new EditKeyframeValue(kf) { IsDone = true };
			original.Add(42);
			
			undo.PerformUndo();

			Assert.IsTrue(!ReferenceEquals(kf.value, original), "Reference is same");
			Assert.AreEqual(1, kf.value.Count);
			Assert.IsTrue(kf.value[0] == 123);
		}
		
		[Test]
		public void Undo_ValueTypeValue_ProducesOriginal()
		{
			var kf = new CustomKeyframe<int>();
			var original = kf.value = 42;

			ICommand undo = new EditKeyframeValue(kf) { IsDone = true };
			var changed = kf.value = 99;
			
			Assert.IsTrue(kf.value == changed, "Starting state is wrong");
			undo.PerformUndo();

			Assert.IsTrue(kf.value == original, "Value is old value again");
		}
	}
}
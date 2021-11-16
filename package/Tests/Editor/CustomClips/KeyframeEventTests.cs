using NUnit.Framework;

namespace Needle.Timeline.Tests.CustomClips
{
	public class KeyframeEventTests
	{
		[Test]
		public void EditingTime_RaisesEvent()
		{
			var count = 0;
			var kf = new CustomKeyframe<int>();
			kf.time = 0;
			kf.TimeChanged += OnEvent;
			void OnEvent() => count += 1;

			kf.time += 10;
			
			Assert.AreEqual(1, count, "No event");
		}
		
		[Test]
		public void EditingTimeWithoutChangingValue_DoesNot_RaiseEvent()
		{
			var count = 0;
			var kf = new CustomKeyframe<int>();
			kf.time = 1;
			kf.TimeChanged += OnEvent;
			void OnEvent() => count += 1;

			kf.time = 1;
			
			Assert.AreEqual(0, count, "Event called");
		}
		
		[Test]
		public void EditingValue_DoesNot_RaiseEventTimeEvent()
		{
			var count = 0;
			var kf = new CustomKeyframe<int>();
			kf.time = 1;
			kf.TimeChanged += OnEvent;
			void OnEvent() => count += 1;

			kf.value += 1000;
			
			Assert.AreEqual(0, count, "Event called");
		}
		
		[Test]
		public void EditingValue_RaisesEvent()
		{
			var count = 0;
			var kf = new CustomKeyframe<int>();
			kf.value = 15;
			kf.ValueChanged += OnEvent;
			void OnEvent() => count += 1;

			kf.value += 1;

			Assert.AreEqual(1, count, "Event called");
		}
		
		[Test]
		public void EditingValueWithoutChangingValue_DoesNot_RaiseEvent()
		{
			var count = 0;
			var kf = new CustomKeyframe<int>();
			kf.value = 15;
			kf.ValueChanged += OnEvent;
			void OnEvent() => count += 1;

			kf.value = 15;

			Assert.AreEqual(0, count, "Event called");
		}
	}
}
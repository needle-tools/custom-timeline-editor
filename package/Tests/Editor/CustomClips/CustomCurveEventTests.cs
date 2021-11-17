using NUnit.Framework;

namespace Needle.Timeline.Tests.CustomClips
{
	public class CustomCurveEventTests
	{
		[Test]
		public void AddingKeyframe_RaisesEvent()
		{
			var count = 0;
			var clip = new CustomAnimationCurve<int>();
			clip.Changed += OnEvent;
			void OnEvent(ICustomClip _) => count += 1;

			clip.Add(new CustomKeyframe<int>(0, 0));

			Assert.AreEqual(1, count, "Event count mismatch");

			clip.Add(new CustomKeyframe<int>(100, 10));
				
			Assert.AreEqual(2, count, "Event count mismatch");
		}
		
		[Test]
		public void RemovingKeyframe_RaisesEvent()
		{
			var count = 0;
			var clip = new CustomAnimationCurve<int>();
			var kf = new CustomKeyframe<int>(0, 0);
			clip.Add(kf);
			clip.Changed += OnEvent;
			void OnEvent(ICustomClip _) => count += 1;

			clip.Remove(kf);

			Assert.AreEqual(1, count, "Event count mismatch");
		}
		
		[Test]
		public void ChangingKeyframeTime_RaisesEvent()
		{
			var count = 0;
			var kf = new CustomKeyframe<int>(0, 0);
			var clip = new CustomAnimationCurve<int>();
			clip.Add(kf);
			clip.Changed += OnEvent;
			void OnEvent(ICustomClip _) => count += 1;

			kf.time += 10;

			Assert.AreEqual(1, count, "Event count mismatch");
		}
		
		[Test]
		public void ChangingKeyframeValue_RaisesEvent()
		{
			var count = 0;
			var kf = new CustomKeyframe<int>(0, 0);
			var clip = new CustomAnimationCurve<int>();
			clip.Add(kf);
			clip.Changed += OnEvent;
			void OnEvent(ICustomClip _) => count += 1;

			kf.value += 10;

			Assert.AreEqual(1, count, "Event count mismatch");
		}
		
		[Test]
		public void ChangingRemovedKeyframeValue_DoesNotRaiseEvent()
		{
			var count = 0;
			var kf = new CustomKeyframe<int>(0, 0);
			var clip = new CustomAnimationCurve<int>();
			clip.Add(kf);
			clip.Remove(kf);
			clip.Changed += OnEvent;
			void OnEvent(ICustomClip _) => count += 1;

			kf.value += 10;

			Assert.AreEqual(0, count, "Event count mismatch");
		}
		
		[Test]
		public void ChangingRemovedKeyframeTime_DoesNotRaiseEvent()
		{
			var count = 0;
			var kf = new CustomKeyframe<int>(0, 0);
			var clip = new CustomAnimationCurve<int>();
			clip.Add(kf);
			clip.Remove(kf);
			clip.Changed += OnEvent;
			void OnEvent(ICustomClip _) => count += 1;

			kf.time += 10;

			Assert.AreEqual(0, count, "Event count mismatch");
		}
	}
}
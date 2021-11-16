using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Needle.Timeline.Tests.CustomClips
{
	public class CustomCurveTests
	{
		[Test]
		public void AddingKeyframe_IsAllowed_IfTypeMatches()
		{
			var clip = new CustomAnimationCurve<int>();
			
			var kf = new CustomKeyframe<int>(0, 0);
			var res = clip.Add(kf);
			
			Assert.IsTrue(res);
			Assert.AreEqual(1, clip.Keyframes.Count);
			Assert.IsTrue(clip.Keyframes.Any(k => k == kf), "Keyframe not added");
		}
		
		[Test]
		public void AddingKeyframe_IsNotAllowed_IfKeyframeAtTimeExists()
		{
			var clip = new CustomAnimationCurve<int>();
			
			var kf = new CustomKeyframe<int>(0, 0);
			clip.Add(kf);
			
			var other = new CustomKeyframe<int>(0, 0);
			LogAssert.Expect(LogType.Error, "Keyframe already exists at time 0");
			var res = clip.Add(other);

			Assert.IsFalse(res);
			Assert.AreEqual(1, clip.Keyframes.Count);
			Assert.IsTrue(clip.Keyframes.Any(k => k == kf), "Keyframe not added");
		}
	}
}
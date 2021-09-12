using System;
using DefaultNamespace;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

namespace Needle.Timeline
{
	[Serializable]
	public class CodeControlBehaviour : PlayableBehaviour
	{
		
		
		internal AnimationClip clip;
		internal CodeTrack.ClipInfo bindings;

		public override void OnPlayableCreate(Playable playable)
		{
			base.OnPlayableCreate(playable);
		}

		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
			// boundType = playerData?.GetType();
			// if (clip != null)
			// var go = GameObject.Find("GameObject");

			// if (clip)
			// {
			// 	clip.SetCurve("GameObject", typeof(AnimatedScript), "MyValue", curve);
			// }

			if (clip != null)
			{
				// clip.SampleAnimation(go, (float)playable.GetTime());
				if (playerData is IAnimated anim)
				{
				}


				var clipBindings = AnimationUtility.GetCurveBindings(clip);
				for (var index = 0; index < clipBindings.Length; index++)
				{
					var binding = clipBindings[index];
					var curve = AnimationUtility.GetEditorCurve(clip, binding);
					if (curve != null)
					{
						// Debug.Log(binding.propertyName);
						var value = curve.Evaluate((float)playable.GetTime());
						// Debug.Log(value);
						bindings.values[index]?.SetValue(value);
					}
				}
			}


			base.ProcessFrame(playable, info, playerData);
		}
	}
}
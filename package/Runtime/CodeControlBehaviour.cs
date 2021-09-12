using System;
using _Sample;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

namespace Needle.Timeline
{
	[Serializable]
	public class CodeControlBehaviour : PlayableBehaviour
	{
		// internal Type boundType { get; private set; }

		internal AnimationClip clip;

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
				if (playerData is AnimatedScript anim)
				{
					var clipBindings = AnimationUtility.GetCurveBindings(clip);
					foreach (var binding in clipBindings)
					{
						var curve = AnimationUtility.GetEditorCurve(clip, binding);
						if (curve != null)
						{
							// Debug.Log(binding.propertyName);
							anim.MyValue = curve.Evaluate((float)playable.GetTime());
						}
					}
				}
			}


			base.ProcessFrame(playable, info, playerData);
		}
	}
}
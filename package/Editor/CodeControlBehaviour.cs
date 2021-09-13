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
		internal ClipInfoModel bindings;
		
		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
			if (bindings != null)
			{
				var time = (float)playable.GetTime();
				for (var index = 0; index < bindings.clips.Count; index++)
				{
					var curve = bindings.clips[index];
					var val = curve.Evaluate(time);
					bindings.values[index].SetValue(val);
				}

				// var clipBindings = AnimationUtility.GetCurveBindings(bindings.clip);
				// for (var index = 0; index < clipBindings.Length; index++)
				// {
				// 	var binding = clipBindings[index];
				// 	var curve = AnimationUtility.GetEditorCurve(bindings.clip, binding);
				// 	if (curve != null)
				// 	{
				// 		// Debug.Log(binding.propertyName);
				// 		var value = curve.Evaluate((float)playable.GetTime());
				// 		// Debug.Log(value);
				// 		bindings.values[index]?.SetValue(value);
				// 	}
				// }
			}


			base.ProcessFrame(playable, info, playerData);
		}
	}
}
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

namespace Needle.Timeline
{
	[Serializable]
	public class CodeControlBehaviour : PlayableBehaviour
	{
		internal List<ClipInfoViewModel> viewModels;
		
		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
			// if (CodeControlTrack.IsUsingMixer) return;
			//
			// Debug.Log("Process frame " + info.frameId);
			// if (viewModel != null)
			// {
			// 	var time = (float)playable.GetTime();
			// 	for (var index = 0; index < viewModel.clips.Count; index++)
			// 	{
			// 		var curve = viewModel.clips[index];
			// 		var val = curve.Evaluate(time);
			// 		viewModel.values[index].SetValue(val);
			// 	}
			// }
			//
			//
			// base.ProcessFrame(playable, info, playerData);
		}
	}
}
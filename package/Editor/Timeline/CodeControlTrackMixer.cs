using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Playables;

namespace Needle.Timeline
{
	public class CodeControlTrackMixer : PlayableBehaviour
	{
		private readonly ProfilerMarker mixerMarker = new ProfilerMarker(nameof(CodeControlTrackMixer));
		private readonly List<object> valuesToMix = new List<object>();

		// NOTE: This function is called at runtime and edit time.  Keep that in mind when setting the values of properties.
		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
			using(mixerMarker.Auto())
			{
				valuesToMix.Clear();
				var inputCount = playable.GetInputCount();

				for (var i = 0; i < inputCount; i++)
				{
					var inputWeight = playable.GetInputWeight(i);
					if (inputWeight <= 0.000001f) continue;

					var inputPlayable = (ScriptPlayable<CodeControlBehaviour>)playable.GetInput(i);
					var behaviour = inputPlayable.GetBehaviour();
					if (behaviour.viewModel == null) continue;

					var time = (float)((playable.GetTime() - behaviour.viewModel.startTime) * behaviour.viewModel.timeScale);
				
					// Debug.Log("Mix frame " + info.frameId);
					var viewModel = behaviour.viewModel;
					var saveToMix = inputWeight < 1f && valuesToMix.Count <= 0;
					for (var index = 0; index < viewModel.clips.Count; index++)
					{
						var curve = viewModel.clips[index];
						var val = curve.Evaluate(time);
						if (saveToMix)
						{
							valuesToMix.Add(val);
						}
						else if (inputWeight < 1f && valuesToMix.Count > index)
						{
							var prev = valuesToMix[index];
							// Debug.Log("MIX " + inputWeight);
							var final = curve.Interpolate(prev, val, inputWeight);
							// if (prev is List<Vector3> list0 && val is List<Vector3> list1 && final is List<Vector3> res)
							// {
							// 	Debug.Log(list0.Count + " > " + list1.Count + ", " + res.Count + ", " + inputWeight.ToString("0.0"));
							// }
							viewModel.values[index].SetValue(final);
						}
						else
						{
							// if (val is List<Vector3> list1)
							// 	Debug.Log(list1.Count);
							viewModel.values[index].SetValue(val);
						}
					}
				}
			}
		}
	}
}
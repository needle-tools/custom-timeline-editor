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

					var viewModel = behaviour.viewModel;
					var length = (float)viewModel.director.duration;
					var time = (float)behaviour.viewModel.ToClipTime(playable.GetTime()); //((playable.GetTime() - behaviour.viewModel.startTime) * behaviour.viewModel.timeScale);
					// Debug.Log(time.ToString("0.0") + ", " + length.ToString("0.0"));
					// looping support:
					time %= (length * (float)behaviour.viewModel.timeScale);
				
					// Debug.Log("Mix frame " + info.frameId);
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
							var final = curve.Interpolate(prev, val, inputWeight);
							viewModel.values[index].SetValue(final);
						}
						else
						{
							viewModel.values[index].SetValue(val);
						}
					}
				}
			}
		}
	}
}
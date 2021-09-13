using Unity.Profiling;
using UnityEditor.Profiling;
using UnityEngine;
using UnityEngine.Playables;

namespace Needle.Timeline
{
	public class CodeControlTrackMixer : PlayableBehaviour
	{
		private ProfilerMarker mixerMarker = new ProfilerMarker(nameof(CodeControlTrackMixer));
		
		// NOTE: This function is called at runtime and edit time.  Keep that in mind when setting the values of properties.
		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
			using(mixerMarker.Auto())
			{
				var inputCount = playable.GetInputCount();
				var time = (float)playable.GetTime();

				for (var i = 0; i < inputCount; i++)
				{
					var inputWeight = playable.GetInputWeight(i);
					if (inputWeight <= 0.000001f) continue;

					var inputPlayable = (ScriptPlayable<CodeControlBehaviour>)playable.GetInput(i);
					var behaviour = inputPlayable.GetBehaviour();
					if (behaviour.viewModel == null) continue;
				
					// Debug.Log("Mix frame " + info.frameId);
					var viewModel = behaviour.viewModel;
					for (var index = 0; index < viewModel.clips.Count; index++)
					{
						var curve = viewModel.clips[index];
						var val = curve.Evaluate(time);

						switch (val)
						{
							case float fl:
								val = fl * inputWeight;
								break;
						}

						viewModel.values[index].SetValue(val);
					}
				}
			}
		}
	}
}
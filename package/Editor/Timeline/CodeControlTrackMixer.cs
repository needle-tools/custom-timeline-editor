using System.Collections.Generic;
using System.Linq;
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

		private Dictionary<PlayableDirector, int> _directorTime;
		
		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
			using var auto = mixerMarker.Auto();
			
			var inputCount = playable.GetInputCount();
			var inputPlayable = (ScriptPlayable<CodeControlBehaviour>)playable.GetInput(0);
			var behaviour = inputPlayable.GetBehaviour();
			for (var viewModelIndex = 0; viewModelIndex < behaviour.viewModels.Count; viewModelIndex++)
			{
				// var viewModel = behaviour.viewModels[viewModelIndex];
				valuesToMix.Clear();

				for (var i = 0; i < inputCount; i++)
				{
					var inputWeight = playable.GetInputWeight(i);
					if (inputWeight <= 0.000001f) continue;

					inputPlayable = (ScriptPlayable<CodeControlBehaviour>)playable.GetInput(i);
					var b = inputPlayable.GetBehaviour();

					var viewModel = b.viewModels[viewModelIndex];
					var soloing = b.viewModels.Where(s => s.Solo && s.IsValid && s.currentlyInClipTime);
					var anySolo = soloing.Any();

					if (!viewModel.IsValid) continue;
					if (anySolo && !viewModel.Solo) continue;

					if (viewModel.Solo) inputWeight = 1;

					// Debug.Log(viewModel.Script + ", " + inputWeight + ", " + viewModel.clips.Count);
					var length = (float)viewModel.director.duration;
					var time = (float)viewModel.ToClipTime(playable
						.GetTime()); //((playable.GetTime() - behaviour.viewModel.startTime) * behaviour.viewModel.timeScale);
					// Debug.Log(time.ToString("0.0") + ", " + length.ToString("0.0"));
					// looping support:
					time %= (length * (float)viewModel.timeScale);

					// Debug.Log("Mix frame " + info.frameId);
					var saveToMix = inputWeight < 1f && valuesToMix.Count <= 0;
					for (var index = 0; index < viewModel.clips.Count; index++)
					{
						var clip = viewModel.clips[index];
						// Debug.Log(clip);
						var val = clip.Evaluate(time);
						if (saveToMix)
						{
							valuesToMix.Add(val);
						}
						else if (inputWeight < 1f && valuesToMix.Count > index)
						{
							var prev = valuesToMix[index];
							var final = clip.Interpolate(prev, val, inputWeight);
							viewModel.values[index].SetValue(final);
						}
						else
						{
							viewModel.values[index].SetValue(val);
						}
					}
					// break;
				}
				
			
				var graph = playable.GetGraph();
				if (graph.GetResolver() is PlayableDirector dir)
				{
					TimelineHooks.CheckTimeChanged(dir);
				}
			}
		}
	}
}
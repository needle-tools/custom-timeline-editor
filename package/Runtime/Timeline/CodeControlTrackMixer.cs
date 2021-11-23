using System.Collections.Generic;
using System.Linq;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

namespace Needle.Timeline
{
	public class CodeControlTrackMixer : PlayableBehaviour
	{
		private readonly ProfilerMarker mixerMarker = new ProfilerMarker(nameof(CodeControlTrackMixer));
		private readonly List<object> valuesToMix = new List<object>();

		private static float fixedDeltaTime;

		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
			using var auto = mixerMarker.Auto();

			var inputCount = playable.GetInputCount();
			var inputPlayable = (ScriptPlayable<CodeControlBehaviour>)playable.GetInput(0);
			var behaviour = inputPlayable.GetBehaviour();
			// if not bound
			if (behaviour == null) return;
			var frameInfo = new FrameInfo((float)playable.GetTime(), info.deltaTime);
			for (var viewModelIndex = 0; viewModelIndex < behaviour.asset.viewModels.Count; viewModelIndex++)
			{
				// var viewModel = behaviour.viewModels[viewModelIndex];
				valuesToMix.Clear();
 
				for (var i = 0; i < inputCount; i++)
				{
					var inputWeight = playable.GetInputWeight(i);
					if (inputWeight <= 0.000001f) continue; 

					inputPlayable = (ScriptPlayable<CodeControlBehaviour>)playable.GetInput(i);
					var b = inputPlayable.GetBehaviour();
					if (b.asset.viewModels == null || viewModelIndex >= b.asset.viewModels.Count)
					{
						// TODO: should set everything to null if no values exist?
						// TODO: handle post extrapolate
						continue;
					}
					var viewModel = b.asset.viewModels[viewModelIndex];
					viewModel.Script.OnProcessFrame(frameInfo);

					var soloing = ClipInfoViewModel.Instances.Where(s => s.Solo && s.IsValid && s.currentlyInClipTime);
					var anySolo = soloing.Any();

					if (!viewModel.IsValid) continue;
					if (anySolo && !viewModel.Solo) continue;

					if (viewModel.Solo) inputWeight = 1;

					// Debug.Log(viewModel.Script + ", " + inputWeight + ", " + viewModel.clips.Count);
					var length = (float)viewModel.director.duration;
					var time = (float)viewModel.ToClipTime(playable.GetTime()); 
					//((playable.GetTime() - behaviour.viewModel.startTime) * behaviour.viewModel.timeScale);
					// Debug.Log(time.ToString("0.0") + ", " + length.ToString("0.0"));
					// looping support:
					time %= (length * (float)viewModel.timeScale);

					// Debug.Log("Mix frame " + info.frameId);
					var saveToMix = inputWeight < 1f && valuesToMix.Count <= 0;
					for (var index = 0; index < viewModel.clips.Count; index++)
					{
						var clip = viewModel.clips[index];
						clip.ViewModel = viewModel;
						var val = clip.Evaluate(time);
						if (saveToMix)
						{
							valuesToMix.Add(val);
						}
						else if (inputWeight < 1f && valuesToMix.Count > index)
						{
							var prev = valuesToMix[index];
							var next = val;
							var final = clip.Interpolate(prev, next, inputWeight);
							viewModel.values[index].SetValue(final);
							if(viewModel.Script is IOnionSkin)
								viewModel.StoreEvaluatedResult(viewModel.values[index], clip, final);
						}
						else
						{
							viewModel.values[index].SetValue(val);
							if(viewModel.Script is IOnionSkin)
								viewModel.StoreEvaluatedResult(viewModel.values[index], clip, val);
						}
					}
				}


				// if (!Application.isPlaying)
				// {
				// 	var allowPhysicsUpdate = !Physics.autoSimulation;
				// 	if(allowPhysicsUpdate)
				// 		Physics.Simulate(Time.fixedDeltaTime);
				// }
				
				var vm = ClipInfoViewModel.Instances[viewModelIndex];
				if (vm.IsValid)
				{ 
					var graph = playable.GetGraph();
					if (graph.GetResolver() is PlayableDirector dir)
					{
						TimelineHooks.CheckStateChanged(dir);
						var now = new FrameInfo((float)playable.GetTime(), info.deltaTime);
						vm.OnProcessedFrame(now);
					}
					else
					{
						vm.OnProcessedFrame(frameInfo);
					}
				}
			}
		}

#if UNITY_EDITOR
		[DrawGizmo(GizmoType.NonSelected | GizmoType.Selected)]
		private static void DrawGizmos(PlayableDirector component, GizmoType gizmoType)
		{
			foreach (var vm in ClipInfoViewModel.ActiveInstances)
			{
				if(vm.IsValid)
					vm.RenderDataPreview(CustomTimelineSettings.Instance.RenderOnionSkin);
			}
		}
#endif
	}
}
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace Needle.Timeline
{
	[CustomTimelineEditor(typeof(CodeControlAsset))]
	public class CodeControlAssetEditor : ClipEditor
	{
		public override void OnCreate(TimelineClip clip, TrackAsset track, TimelineClip clonedFrom)
		{
			base.OnCreate(clip, track, clonedFrom);
			if (clonedFrom == null)
			{
				EditorApplication.delayCall += () => SyncViewModelState(track, clip);
				return;
			}

			ClipInfoViewModel source = null;
			const float maxTimeDifference = float.Epsilon;
			foreach (var vm in ClipInfoViewModel.Instances)
			{
				if (source != null) break;
				if (Math.Abs(vm.startTime - clonedFrom.start) < maxTimeDifference 
				    && Math.Abs(vm.endTime - clonedFrom.end) < maxTimeDifference) 
					source = vm;
			}
			if(source != null)
				EditorApplication.delayCall += () => PopulateClip(source, clip);
		}

		private static async void SyncViewModelState(TrackAsset track, TimelineClip clip)
		{
			await Task.Delay(10);
			// TODO: move states like that OUT of clips, it should be serialized with the track
			var vm = ClipInfoViewModel.Instances.FirstOrDefault(vm => vm.asset == clip.asset);
			if (vm != null)
			{
				var anyOther = ClipInfoViewModel.Instances.FirstOrDefault(cm => cm != vm);
				if (anyOther != null && vm.clips.Count == anyOther.clips.Count)
				{
					Debug.Log("FIXME: syncing recording states here but should actually be serialized with track");
					for (var index = 0; index < vm.clips.Count; index++)
					{
						var c = vm.clips[index];
						var other = anyOther.clips[index];
						if (c is IRecordable r && other is IRecordable or)
						{
							r.IsRecording = or.IsRecording;
						}
					}
				}
			}
		}

		private static async void PopulateClip(ClipInfoViewModel source, TimelineClip createdClip)
		{
			// copy paste works immediately but duplicate seems to be delayed?!
			// so we wait
			await Task.Delay(10);
			
			ClipInfoViewModel created = null;
			foreach (var vm in ClipInfoViewModel.Instances) 
			{ 
				if (createdClip.asset != vm.asset) continue;
				created = vm;
			}
			if (created == null) return;
			
			
			for (var index = 0; index < source.clips.Count; index++)
			{
				var clip = source.clips[index];
				var clone = AnimationCurveBuilder.Clone(created, clip);
				created.Replace(created.clips[index], clone);
				created.HasUnsavedChanges = true;
			}
		}
	}
}
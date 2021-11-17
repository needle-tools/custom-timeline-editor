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
			if (clonedFrom == null) return;

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

		private static void PopulateClip(ClipInfoViewModel source, TimelineClip createdClip)
		{
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
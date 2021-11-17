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
			Debug.Log("TODO: clone clips");
			
			
			ClipInfoViewModel source = null;
			foreach (var vm in ClipInfoViewModel.Instances)
			{
				const float TOLERANCE = float.Epsilon;
				if (Math.Abs(vm.startTime - clonedFrom.start) < TOLERANCE && Math.Abs(vm.endTime - clonedFrom.end) < TOLERANCE) 
					source = vm;
			}
			if(source != null)
				EditorApplication.delayCall += () => PopulateClip(source, clip);
			
		}

		private static async void PopulateClip(ClipInfoViewModel source, TimelineClip createdClip)
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
				// created.clips.Add(clip);
				var clone = CloneUtil.TryClone(clip);
				clone.Id = created.ToId(clone);
				created.Replace(created.clips[index], clone);
				created.HasUnsavedChanges = true;  
			}
		}
	}
}
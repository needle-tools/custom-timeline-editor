using System;
using System.Collections.Generic;
using _Sample;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Needle.Timeline
{
	[TrackClipType(typeof(CodeControlAsset))]
	[TrackBindingType(typeof(MonoBehaviour))]
	public class CodeTrack : TrackAsset
	{
		[SerializeField] 
		private List<ClipInfo> clips = new List<ClipInfo>();

		[Serializable]
		private struct ClipInfo
		{
			public string name;
			public string fullTypeName;
			public AnimationClip clip;
		}

		// internal override Playable CompileClips(PlayableGraph graph, GameObject go, IList<TimelineClip> timelineClips, IntervalTree<RuntimeElement> tree)
		// {
		// 	return base.CompileClips(graph, go, timelineClips, tree);
		// }

		protected override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip clip)
		{
			Debug.Log("<b>Create Playable</b>");
			
			
			clip.CreateCurves("Procedural");
			var curve = AnimationCurve.Linear(0, 0, 1, 1);
			clip.curves.SetCurve("GameObject", typeof(AnimatedScript), "MyValue", curve);
			if (clip.asset is CodeControlAsset asset)
			{
				asset.clip = clip.curves;
			}
			return base.CreatePlayable(graph, gameObject, clip);
		}


		// public TimelineClip CreateRecordableClip(string animClipName)
		// {
			// var clip = TimelineCreateUtilities.CreateAnimationClipForTrack(string.IsNullOrEmpty(animClipName) ? "TEST" : animClipName, this, false);
		// 
		// 	var timelineClip = CreateClip(clip);
		// 	timelineClip.displayName = animClipName;
		// 	timelineClip.recordable = true;
		// 	timelineClip.start = 0;
		// 	timelineClip.duration = 1;
		//
		//
		// 	var apa = timelineClip.asset as AnimationPlayableAsset;
		// 	if (apa != null)
		// 		apa.removeStartOffset = false;
		//
		// 	return timelineClip;
		// }
		//
		// public TimelineClip CreateClip(AnimationClip clip)
		// {
		// 	if (clip == null)
		// 		return null;
		//
		// 	var newClip = CreateClip<CodeControlAsset>();
		// 	AssignAnimationClip(newClip, clip);
		// 	return newClip;
		// }
		//
		//
		// void AssignAnimationClip(TimelineClip clip, AnimationClip animClip)
		// {
		// 	if (clip == null || animClip == null)
		// 		return;
		//
		// 	if (animClip.legacy)
		// 		throw new InvalidOperationException("Legacy Animation Clips are not supported");
		//
		// 	CodeControlAsset asset = clip.asset as CodeControlAsset;
		// 	if (asset != null)
		// 	{
		// 		asset.clip = animClip;
		// 		asset.name = animClip.name;
		// 		var duration = asset.duration;
		// 		if (!double.IsInfinity(duration) && duration >= TimelineClip.kMinDuration && duration < TimelineClip.kMaxTimeValue)
		// 			clip.duration = duration;
		// 	}
		//
		// 	clip.displayName = animClip.name;
		// }
	}
}
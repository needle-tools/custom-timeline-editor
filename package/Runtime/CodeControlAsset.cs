using System.Collections.Generic;
using System.Data;
using _Sample;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Needle.Timeline
{
	[System.Serializable]
	public class CodeControlAsset : PlayableAsset, ITimelineClipAsset
	{
		public CodeControlBehaviour template;

		internal CodeControlBehaviour instance;
		public AnimationClip clip;
		
		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{
			var scriptPlayable = ScriptPlayable<CodeControlBehaviour>.Create(graph, template);
			instance = scriptPlayable.GetBehaviour();
			instance.clip = clip;
			
			return scriptPlayable;
		}

		// public void GatherProperties(PlayableDirector director, IPropertyCollector driver)
		// {
		// 	// Debug.Log(clip.empty);
		// 	// driver.AddFromClip(clip);
		// 	// driver.AddFromClip(instance.clip);
		// }
		//
		// public ClipCaps clipCaps => ClipCaps.None;
		public ClipCaps clipCaps => ClipCaps.All;
	}
}
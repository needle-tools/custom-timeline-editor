using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Needle.Timeline
{
	[Serializable]
	public class CodeControlAsset : PlayableAsset, ITimelineClipAsset
	{
		[SerializeField, HideInInspector]
		private CodeControlBehaviour template;

		internal readonly List<ClipInfoViewModel> viewModels = new List<ClipInfoViewModel>();

		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{
			var scriptPlayable = ScriptPlayable<CodeControlBehaviour>.Create(graph, template);
			var b = scriptPlayable.GetBehaviour();
			b.viewModels = viewModels;
			// Debug.Log("Create Playable: " + b.viewModels.Count);
			return scriptPlayable; 
		}

		public ClipCaps clipCaps => ClipCaps.All; 
	}
}
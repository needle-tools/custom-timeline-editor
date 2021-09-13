using System;
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

		internal ClipInfoViewModel viewModel;
		
		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{
			var scriptPlayable = ScriptPlayable<CodeControlBehaviour>.Create(graph, template);
			var b = scriptPlayable.GetBehaviour();
			b.viewModel = viewModel;
			return scriptPlayable; 
		}

		public ClipCaps clipCaps => ClipCaps.All;
	}
}
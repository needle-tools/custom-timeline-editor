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
		[SerializeField, HideInInspector] private CodeControlBehaviour template;

		[SerializeField] internal string id;
		[SerializeField] private List<JsonContainer> clipData = new List<JsonContainer>();

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

		internal JsonContainer TryFind(string clipId)
		{
			foreach (var e in clipData)
			{
				if (e.Id == clipId) return e;
			}
			return null;
		}

		internal void AddOrUpdate(JsonContainer container)
		{
			clipData.RemoveAll(c => !c);
			if (clipData.Contains(container)) return;
			clipData.Add(container);
		}
	}
}
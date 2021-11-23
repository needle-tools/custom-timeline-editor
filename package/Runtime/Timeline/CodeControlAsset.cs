using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Needle.Timeline
{
	[Serializable]
	public class CodeControlAsset : PlayableAsset, ITimelineClipAsset
	{
		[SerializeField, HideInInspector] internal string id;
		[SerializeField] internal CodeControlAssetData data;
		internal readonly List<ClipInfoViewModel> viewModels = new List<ClipInfoViewModel>();
		internal static event Action<CodeControlAsset> Deleted;
		
		[Header("LEGACY")]
		[SerializeField, Obsolete] internal List<JsonContainer> clipData = new List<JsonContainer>();
		
		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{
			var scriptPlayable = ScriptPlayable<CodeControlBehaviour>.Create(graph);
			var b = scriptPlayable.GetBehaviour();
			b.viewModels = viewModels;
			return scriptPlayable;
		}

		public ClipCaps clipCaps => ClipCaps.All;

		internal JsonContainer TryFind(string clipId)
		{
			if (!data || data.ClipData == null) return null;
			foreach (var e in data.ClipData)
			{
				if (e.Id == clipId) return e;
			}
			return null;
		}
		//
		// internal void AddOrUpdate(JsonContainer container)
		// {
		// 	clipData.RemoveAll(c => !c);
		// 	if (clipData.Contains(container)) return;
		// 	clipData.Add(container);
		// }
		

		private void OnDestroy()
		{
			Deleted?.Invoke(this);
		}
	}
}
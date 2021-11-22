﻿using System;
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
		[SerializeField] internal CodeControlAssetData data;
		[SerializeField] internal string id;
		[SerializeField] internal List<JsonContainer> clipData = new List<JsonContainer>();
		internal readonly List<ClipInfoViewModel> viewModels = new List<ClipInfoViewModel>();
		internal static event Action<CodeControlAsset> Deleted;
		
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
		

		private void OnDestroy()
		{
			Deleted?.Invoke(this);
		}
	}
}
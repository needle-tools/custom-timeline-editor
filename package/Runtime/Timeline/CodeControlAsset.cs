using System;
using System.Collections.Generic;
using JetBrains.Annotations;
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
		internal static event Action<CodeControlAsset> Deleted;

		private bool created;
		internal List<ClipInfoViewModel> viewModels = new List<ClipInfoViewModel>();

		internal void AddViewModel(ClipInfoViewModel vm)
		{
			if (viewModels.Contains(vm)) return;
			viewModels.Add(vm);
		}

		[Header("LEGACY - dont use anymore")] [SerializeField, Obsolete]
		internal List<JsonContainer> clipData;
		
		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{ 
			var scriptPlayable = ScriptPlayable<CodeControlBehaviour>.Create(graph);
			var b = scriptPlayable.GetBehaviour();
			b.asset = this;
			created = true;
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

		private CodeControlAssetData previousData;
		[CanBeNull] private List<ClipInfoViewModel> previousViewModels;
		
		private void OnValidate()
		{
			if (!created)
			{
				previousData = data;
				return; 
			}
			if (data != previousData)
			{
				if (previousData && !data || (previousData && data))
				{ 
					// Debug.Log("DATA REPLACED"); 
					previousViewModels ??= new List<ClipInfoViewModel>();
					previousViewModels.Clear();
					previousViewModels.AddRange(viewModels); 
					viewModels.Clear();
					foreach (var vm in previousViewModels) vm.Unregister();
				}   
				else
				{
					// Debug.Log("DATA ADDED"); 
					viewModels.Clear(); 
					if (previousViewModels != null)
					{
						viewModels.AddRange(previousViewModels);
						previousViewModels.Clear();
						foreach (var vm in viewModels) vm.Register();
					}
				}
			}  
			 
			previousData = data;
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
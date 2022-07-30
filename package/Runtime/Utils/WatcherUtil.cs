using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Needle.Timeline
{
	[InitializeOnLoad]
	internal static class UpdateWatcherUtil
	{
		internal static void Register(Object obj, Action callback)
		{
			var id = GetId(obj);
			if (id == null) return;
			var entry = (id, callback);
			if (watchList.Contains(entry)) return;
			Debug.Log("Start watching " + obj, obj);
			watchList.Add(entry);
		}

		internal static void Unregister(Object obj, Action callback)
		{
			var id = GetId(obj);
			if (id == null) return;
			watchList.Remove((id, callback));
		}

		private static readonly List<(string id, Action callback)> watchList = new List<(string id, Action callback)>();

		static UpdateWatcherUtil()
		{
			watchList.Clear();
		}

		[RuntimeInitializeOnLoadMethod]
		private static void Init()
		{
			watchList.Clear();
		}

		private static string GetId(Object obj)
		{
#if UNITY_EDITOR
			if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out var guid, out long id)) return null;
			return guid;
#else
			return null;
#endif
		}

#if UNITY_EDITOR
		private class ImportWatcher : UnityEditor.Experimental.AssetsModifiedProcessor
		{
			protected override void OnAssetsModified(string[] changedAssets,
				string[] addedAssets,
				string[] deletedAssets,
				UnityEditor.Experimental.AssetMoveInfo[] movedAssets)
			{
				foreach (var ch in changedAssets)
				{
					var path = AssetDatabase.AssetPathToGUID(ch);
					foreach (var e in watchList)
					{
						if (e.id == path)
						{
							Debug.Log("CHANGED: " + ch);
							if(e.callback != null) e.callback.Invoke();
							else if (TimelineBuffer.Enabled)
							{
#pragma warning disable CS4014
								TimelineBuffer.RequestBufferCurrentInspectedTimeline();
#pragma warning restore CS4014
							}
						}
					}

					if (ch.EndsWith(".compute"))
					{
						RaiseComputeShaderChangedEventDelayed(ch);
					}
				}
			}

			private async void RaiseComputeShaderChangedEventDelayed(string path)
			{
				if (ComputeShaderUtils.CanRaiseShaderChangedEvent)
				{
					await Task.Delay(10);
					var shader = AssetDatabase.LoadAssetAtPath<ComputeShader>(path);
					shader.RaiseShaderChangedEvent();
				}
			} 
		}
#endif
	}
}
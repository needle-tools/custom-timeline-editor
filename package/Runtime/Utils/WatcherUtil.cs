using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Needle.Timeline
{
	internal static class UpdateWatcherUtil
	{
		internal static void Register(Object obj)
		{
			var id = GetId(obj);
			if (id == null) return;
			if (watchList.Contains(id)) return;
			Debug.Log("Start watching " + obj, obj);
			watchList.Add(id);
		}

		internal static void Unregister(Object obj)
		{
			var id = GetId(obj);
			if (id == null) return;
			watchList.Remove(id);
		}

		private static readonly List<string> watchList = new List<string>();

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
					if (watchList.Contains(path))
					{
						Debug.Log("CHANGED: " +ch);
#pragma warning disable CS4014
						if(TimelineBuffer.Enabled)
							TimelineBuffer.RequestBufferCurrentInspectedTimeline(30);
#pragma warning restore CS4014
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
﻿using System.Collections.Generic;
using UnityEditor.Experimental;
using UnityEngine;

namespace Needle.Timeline
{
	// ReSharper disable once ClassNeverInstantiated.Global
	public class UnitySaveUtil : AssetsModifiedProcessor 
	{
		protected override void OnAssetsModified(string[] changedAssets, string[] addedAssets, string[] deletedAssets, AssetMoveInfo[] movedAssets)
		{
			foreach (var ch in changedAssets)
			{
				if (tracks.TryGetValue(ch, out var list))
				{
					Debug.Log("Track changed: " + ch);
					foreach (var track in list)
					{
						track.Save();
					}
				}
				// else Debug.Log(ch); 
			}
		}

		private static readonly Dictionary<string, List<CodeControlTrack>> tracks = new Dictionary<string, List<CodeControlTrack>>();

		internal static void Register(CodeControlTrack track, string path)
		{
			if(!tracks.ContainsKey(path)) tracks.Add(path, new List<CodeControlTrack>());
			var list = tracks[path];
			if(!list.Contains(track)) list.Add(track);
		}
	}
}
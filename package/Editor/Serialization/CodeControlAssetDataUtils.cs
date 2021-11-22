using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Needle.Timeline.Serialization
{
	internal static class CodeControlAssetDataUtils
	{
		public static void ExportAsAsset(this CodeControlAsset asset)
		{
			if (!asset) return;
			var data = ScriptableObject.CreateInstance<CodeControlAssetData>();
			var path = "Assets/" + asset.id + ".asset";
			AssetDatabase.CreateAsset(data, path);
			AssetDatabase.Refresh();
			asset.data = data;
					
			data.ClipData = new List<JsonContainer>();
			foreach (var e in asset.clipData)
			{
				if (!e) continue;
				var copy = ScriptableObject.CreateInstance<JsonContainer>();
				copy.hideFlags = HideFlags.NotEditable;
				copy.Id = e.Id.Substring(e.Id.LastIndexOf("_", StringComparison.Ordinal)+1);
				copy.Content = e.Content;
				copy.name = e.name;
				data.ClipData.Add(copy);
				AssetDatabase.AddObjectToAsset(copy, data);
			}
			AssetDatabase.Refresh();
			Debug.Log("Saved as " + path, data);
		}
	}
}
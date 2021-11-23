using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;

namespace Needle.Timeline.Serialization
{
	internal static class CodeControlAssetDataUtils
	{
		public static void ExportAsAsset(this CodeControlAsset asset)
		{
			if (!asset) return;
			var data = ScriptableObject.CreateInstance<CodeControlAssetData>();
			var basePath = GetSavePath();
			var path = basePath + "/" + asset.id + ".asset";
			path = AssetDatabase.GenerateUniqueAssetPath(path);
			AssetDatabase.CreateAsset(data, path);
			AssetDatabase.Refresh();
			asset.data = data;
			data.ClipData = new List<JsonContainer>();
			foreach (var e in asset.clipData)
			{
				if (!e) continue;
				var copy = ScriptableObject.CreateInstance<JsonContainer>();
				copy.hideFlags = HideFlags.NotEditable;
				copy.Id = e.Id.Substring(e.Id.LastIndexOf("_", StringComparison.Ordinal) + 1);
				copy.Content = e.Content;
				copy.name = e.name;
				data.ClipData.Add(copy);
				AssetDatabase.AddObjectToAsset(copy, data);
			}
			AssetDatabase.Refresh();
			Debug.Log("Saved as " + path, data);
		}


		public static void CreateAndAssignDataAsset(this CodeControlAsset asset)
		{
			var basePath = GetSavePath();
			var path = basePath + "/New timeline clip" + ".asset";
			path = AssetDatabase.GenerateUniqueAssetPath(path);
			var data = ScriptableObject.CreateInstance<CodeControlAssetData>();
			AssetDatabase.CreateAsset(data, path); 
			AssetDatabase.Refresh();
			asset.data = data;
			data.ClipData = new List<JsonContainer>();
		}

		private static string GetSavePath()
		{
			var director = TimelineEditor.inspectedDirector;
			var timeline = director.playableAsset;
			var timelinePath = AssetDatabase.GetAssetPath(timeline);
			var dir = Path.GetDirectoryName(timelinePath);
			return dir;
		}
	}
}
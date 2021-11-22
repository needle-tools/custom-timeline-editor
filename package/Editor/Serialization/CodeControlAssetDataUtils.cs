using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Needle.Timeline.Serialization
{
	internal static class CodeControlAssetDataUtils
	{
		public static void SaveAsAsset(this CodeControlAsset asset)
		{
			var data = ScriptableObject.CreateInstance<CodeControlAssetData>();
			var path = "Assets/" + asset.id + ".asset";
			AssetDatabase.CreateAsset(data, path);
			AssetDatabase.Refresh();
			asset.data = data;
					
			data.ClipData = new List<JsonContainer>();
			foreach (var e in asset.clipData)
			{
				var copy = ScriptableObject.CreateInstance<JsonContainer>();
				copy.hideFlags = HideFlags.NotEditable;
				copy.Id = e.Id;
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
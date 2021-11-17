using System;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Needle.Timeline
{
	internal static class AssetDatabaseUtils
	{
		public static void DeleteSubAssets(Object asset, Predicate<Object> delete = null)
		{
			if (!EditorUtility.IsPersistent(asset)) return;
			var path = AssetDatabase.GetAssetPath(asset);
			DeleteSubAssets(path, delete);
		}

		public static void DeleteSubAssets(string path, Predicate<Object> delete = null)
		{
			if (string.IsNullOrEmpty(path)) return;
			var assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
			foreach (var subAsset in assets)
			{ 
				if (!AssetDatabase.IsSubAsset(subAsset)) continue;
				if (delete != null && !delete(subAsset)) continue; 
				Object.DestroyImmediate(subAsset, true);
			}
		}
	}
}
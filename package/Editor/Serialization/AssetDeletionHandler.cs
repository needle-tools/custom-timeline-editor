using UnityEditor;
using UnityEngine;

namespace Needle.Timeline
{
	[InitializeOnLoad]
	internal static class AssetDeletionHandler
	{
		static AssetDeletionHandler()
		{
			CodeControlAsset.Deleted += OnDeleted;
		}

		private static void OnDeleted(CodeControlAsset obj)
		{
			// TODO: we could be nicer here and move assets elsewhere instead of just deleting using some custom undo action but for now that's it 
			var path = AssetDatabase.GetAssetPath(obj);
			AssetDatabaseUtils.DeleteSubAssets(path, p =>
			{
				if (p is JsonContainer json)
				{
					if (json.Id.StartsWith(obj.id))
					{
						Debug.Log("<b>DELETED</b> " + json.Id);
						return true;
					}
				}
				return false;
			});
		}
	}
}
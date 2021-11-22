using System.Collections.Generic;
using Needle.Timeline.Commands;
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

		private static List<Command> commands = new List<Command>();

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
						var deletion = new DeleteTrackCommand(obj, json){IsDone = true};
						commands.Add(deletion);
						EditorApplication.delayCall += RegisterCommands;
						return true;
					}
				}
				return false;
			});
		}

		private static void RegisterCommands()
		{
			if (commands.Count <= 0) return;
			var compound = commands.ToCompound("Deleted tracks");
			commands.Clear();
			Debug.Log("Register " + compound);
			CustomUndo.Register(compound);
		}
	}
}
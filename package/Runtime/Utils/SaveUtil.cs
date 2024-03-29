﻿#nullable enable

using System.IO;
using UnityEngine;

namespace Needle.Timeline
{
	public static class SaveUtil
	{
		private static string? fullLib;
		public static string ProjectSaveDirectory
		{
			get
			{
				return fullLib ??= Path.GetFullPath(Application.dataPath + "/../Timeline");
			}
		}

		private static string? fullSaveDir;
		public static string FullSaveDirectory
		{
			get
			{
				if (fullSaveDir == null)
				{
					fullSaveDir = ProjectSaveDirectory + "/Clips";
				}
				if (!Directory.Exists(fullSaveDir)) Directory.CreateDirectory(fullSaveDir);

				return fullSaveDir;
			}
		}
		
		public static void Save(string id, string json)
		{
			var filePath = Path.Combine(FullSaveDirectory, id + ".json");
			if (File.Exists(filePath)) File.Delete(filePath);
			File.WriteAllText(filePath, json);
		}
		
		public static string? Load(string id)
		{
			var filePath = Path.Combine(FullSaveDirectory, id + ".json");
			if (!File.Exists(filePath)) return null;
			var json = File.ReadAllText(filePath);
			return json;
		}


		public static bool Replace(string oldId, string newId)
		{
			var oldPath = Path.Combine(FullSaveDirectory, oldId + ".json");
			if (File.Exists(oldPath))
			{
				var newPath = Path.Combine(FullSaveDirectory, newId + ".json");
				if (File.Exists(newPath))
				{
					return false;
				}
				Debug.Log("Replacing " + oldId + " with " + newId);
				File.Move(oldPath, newPath);
				return true;
			}
			return false;
		}
	}
}
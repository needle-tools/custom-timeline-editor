using System.IO;
using UnityEditor;
using UnityEngine;

namespace Needle.Timeline
{
	public static class ToolsSettings
	{
		internal static void HandleSettingsForToolInstance(ICustomClipTool instance)
		{
			// var settings = TryLoadSettings(instance);
			// instance.GetOrCreateSettings(ref settings);
			// if (settings)
			// {
			// 	EditorUtility.SetDirty(settings);
			// 	TrySaveSettings(instance, settings);
			// }
		}

		private static readonly string basePath = Application.dataPath + "/../UserSettings/Timeline/Tools";
		private static ScriptableObject TryLoadSettings(ICustomClipTool tool)
		{
			if (Directory.Exists(basePath))
			{
				var path = basePath + "/" + tool.GetType().Name + ".asset";
				var loaded = UnityEditorInternal.InternalEditorUtility.LoadSerializedFileAndForget(path);
				if (loaded != null && loaded.Length == 1)
				{
					return loaded[0] as ScriptableObject;
				}
			}
			return null;
		}

		private static Object[] save;
		private static void TrySaveSettings(ICustomClipTool tool, ScriptableObject settings)
		{
			if (!settings) return;
			if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);
			var path = basePath + "/" + tool.GetType().Name + ".asset";
			save ??= new Object[1];
			save[0] = settings;
			UnityEditorInternal.InternalEditorUtility.SaveToSerializedFileAndForget(save, path, true);

		}
	}
}
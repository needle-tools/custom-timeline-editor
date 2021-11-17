#nullable enable

using System.IO;
using JetBrains.Annotations;
using Needle.Timeline.Serialization;
using UnityEngine;

namespace Needle.Timeline
{
	public class TempFileLocationLoader : ILoader
	{
		internal static string defaultTempSaveDirectory { get; } = Application.dataPath + "/../Temp/Timeline";

		internal static void DeleteTempUnsavedChangesDirectory()
		{
			if (Directory.Exists(defaultTempSaveDirectory))
			{
				Debug.Log("Delete temp saves");
				Directory.Delete(defaultTempSaveDirectory, true);
			}
		}

		private readonly ILoader? fallback;

		private ISerializer serializer;

		ISerializer ILoader.Serializer
		{
			get => serializer;
			set => serializer = value;
		}

		private readonly string basePath;

		public TempFileLocationLoader(ILoader? fallback)
		{
			this.fallback = fallback;
			this.serializer = new NewtonsoftSerializer();
			this.basePath = defaultTempSaveDirectory;
		}

		public bool Save(string id, ISerializationContext context, object @object)
		{
			if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);
			var content = (string)serializer.Serialize(@object);
			var path = basePath + "/" + id + ".json";
			if (File.Exists(path)) File.Delete(path);
			File.WriteAllText(path, content);
			return true;
		}

		public bool Load(string id, ISerializationContext context, out object obj)
		{
			var path = basePath + "/" + id + ".json";
			if (!string.IsNullOrEmpty(path) && File.Exists(path))
			{
				Debug.Log($"Load {context.DisplayName??id} from previously unsaved changes");
				var json = File.ReadAllText(path);
				// File.Delete(path);
				if (!string.IsNullOrEmpty(json))
				{
					obj = serializer.Deserialize(json, context.Type);
					return obj != null;
				}
			}

			if (fallback != null)
			{
				return fallback.Load(id, context, out obj);
			}

			obj = null;
			return false;
		}

		public bool Rename(string oldId, string newId, ISerializationContext context)
		{
			throw new System.NotImplementedException();
		}
	}
}
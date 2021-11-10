using System;
using Needle.Timeline.Serialization;
using UnityEditor;
using UnityEngine;

namespace Needle.Timeline
{
	public class AssetDatabaseProvider : ILoader
	{
		private JsonSerializer serializer;

		
		public AssetDatabaseProvider(ISerializer ser)
		{
			((ILoader)this).Serializer = ser as JsonSerializer;
		}
		
		ISerializer ILoader.Serializer
		{
			get => serializer;
			set
			{
				serializer = value as JsonSerializer;
				if (serializer == null) throw new Exception("Invalid serializer");
			}
		}

		public bool Save(string id, ISerializationContext context, object @object)
		{
			var asset = context.Clip.asset;
			if (!EditorUtility.IsPersistent(asset))
			{
				return false;
			}
			
			var objs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(asset));
			foreach (var obj in objs)
			{
				if (obj is JsonContainer c && c.Id == id)
				{
					c.name = id;
					c.Id = id;
					c.Content = (string)serializer.Serialize(@object);
					EditorUtility.SetDirty(asset);
					return !string.IsNullOrWhiteSpace(c.Content);
				}
			}
			var container = ScriptableObject.CreateInstance<JsonContainer>();
			container.name = id;
			container.Id = id;
			container.Content = (string)serializer.Serialize(@object); 
			AssetDatabase.AddObjectToAsset(container, asset);
			EditorUtility.SetDirty(asset);
			return !string.IsNullOrEmpty(container.Content);
		}

		public bool Load(string id, ISerializationContext context, out object res)
		{
			var asset = context.Clip.asset;
			if (!EditorUtility.IsPersistent(asset))
			{
				res = null;
				return false;
			}
			var objs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(asset));
			foreach (var obj in objs)
			{
				if (obj is JsonContainer c)
				{ 
					if (c.Id == id)
					{
						var json = c.Content;
						if (string.IsNullOrEmpty(json)) continue;
						res = serializer.Deserialize(json, context.Type);
						return res != null;
					} 
				}
			}
			res = null;
			return false;
		}

		public bool Rename(string oldId, string newId)
		{
			return false;
		}
	}
}
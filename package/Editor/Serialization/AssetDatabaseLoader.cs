using System;
using Needle.Timeline.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

namespace Needle.Timeline
{
	[Priority(10)]
	public class AssetDatabaseLoader : ILoader
	{
		// TODO: should be only visible during dev
		internal HideFlags Flags = HideFlags.NotEditable;
		
		private NewtonsoftSerializer serializer;

		
		public AssetDatabaseLoader(ISerializer ser)
		{
			((ILoader)this).Serializer = ser as NewtonsoftSerializer;
		}
		
		ISerializer ILoader.Serializer
		{
			get => serializer;
			set
			{ 
				serializer = value as NewtonsoftSerializer;
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
			foreach (var sub in objs)
			{
				if (sub is JsonContainer json && json.Id == id)
				{
					json.name = GetName();
					json.Id = id;
					json.Content = (string)serializer.Serialize(@object);
					json.hideFlags = Flags;
					EditorUtility.SetDirty(asset);
					SaveForRuntime(json, context.Asset);
					return !string.IsNullOrWhiteSpace(json.Content);
				}
			}
			var container = ScriptableObject.CreateInstance<JsonContainer>();
			container.name = GetName();
			container.Id = id;
			container.Content = (string)serializer.Serialize(@object); 
			container.hideFlags = Flags;
			AssetDatabase.AddObjectToAsset(container, asset);
			EditorUtility.SetDirty(asset);
			SaveForRuntime(container, context.Asset);
			return !string.IsNullOrEmpty(container.Content); 
			
			string GetName() => context.Clip.start.ToString("0.0") + "_" + (context.DisplayName ?? id);
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

		public bool Rename(string oldId, string newId, ISerializationContext context)
		{
			var asset = context.Clip.asset;
			if (!EditorUtility.IsPersistent(asset)) 
			{
				return false;
			}
			
			var objs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(asset));
			foreach (var sub in objs)
			{
				if (sub is JsonContainer json && json.Id == oldId)
				{
					json.name = context.DisplayName ?? newId;
					json.Id = newId;
					json.hideFlags = Flags;
					EditorUtility.SetDirty(asset);
					return true;
				}
			}
			return false;
		}


		private void SaveForRuntime(JsonContainer container, PlayableAsset asset)
		{
			SaveForRuntime(container, asset as CodeControlAsset);
		}

		private void SaveForRuntime(JsonContainer container, CodeControlAsset asset)
		{
			if (!asset || !container)
			{
				Debug.LogError("Not saved for runtime");
				return;
			}
			asset.AddOrUpdate(container);
		}
	}
}
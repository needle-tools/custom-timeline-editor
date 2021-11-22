using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Needle.Timeline.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

namespace Needle.Timeline
{
	[Priority(10)]
	public class AssetDatabaseLoader : ILoader
	{
		private static readonly HideFlags Flags = HideFlags.NotEditable;
		
		
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

			var json = (string)serializer.Serialize(@object);
			if (asset is CodeControlAsset control)
			{
				var dataAsset = control.data;
				if (dataAsset)
				{
					dataAsset.ClipData ??= new List<JsonContainer>();
					foreach (var e in dataAsset.ClipData)
					{
						if (e.Id == id)
						{
							Debug.Log("SAVE TO " + e, e);
							e.name = GetName();
							e.hideFlags = HideFlags.None;// HideFlags.NotEditable;
							e.Content = json;
							EditorUtility.SetDirty(e);
						}
					} 
					EditorUtility.SetDirty(dataAsset);
				}
				return true;
			}
			return false;
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
			
			if (asset is CodeControlAsset control)
			{  
				var dataAsset = control.data; 
				if (dataAsset)
				{
					dataAsset.ClipData ??= new List<JsonContainer>();
					foreach (var e in dataAsset.ClipData)
					{
						e.hideFlags = HideFlags.None;
						if (e.Id == id)
						{  
							var json = e.Content;
							if (string.IsNullOrEmpty(json))
							{
								res = null;
								return false; 
							}
							res = serializer.Deserialize(json, context.Type);
							return res != null;
						} 
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


		// private void SaveForRuntime(JsonContainer container, PlayableAsset asset)
		// {
		// 	SaveForRuntime(container, asset as CodeControlAsset);
		// }
		//
		// private void SaveForRuntime(JsonContainer container, CodeControlAsset asset)
		// {
		// 	if (!asset || !container)
		// 	{
		// 		Debug.LogError("Not saved for runtime");
		// 		return;
		// 	}
		// 	asset.AddOrUpdate(container);
		// }
	}
}
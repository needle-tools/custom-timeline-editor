using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace Needle.Timeline
{
	[Priority(10)]
	public class RuntimeLoader : ILoader
	{
		private ISerializer serializer;

		ISerializer ILoader.Serializer
		{
			get => serializer;
			set => serializer = value;
		}

		public RuntimeLoader(ISerializer ser)
		{
			serializer = ser;
		}

		public bool Load(string id, ISerializationContext context, out object obj)
		{
			if (!context.Asset || !(context.Asset is CodeControlAsset track))
			{
				Debug.LogError("Missing asset or asset is not of right type: " + context.Asset);
				obj = null;
				return false;
			}
			var container = track.TryFind(id);
			if (!container)
			{
				Debug.LogError($"Asset {track.id} does not contain json: {id}");
				obj = null;
				return false;
			}
			obj = serializer.Deserialize(container.Content, context.Type);
			return obj != null;
		}

		public bool Save(string id, ISerializationContext context, object @object)
		{
			throw new System.NotImplementedException();
		}

		public bool Rename(string oldId, string newId, ISerializationContext context)
		{
			throw new System.NotImplementedException();
		}
	}
}
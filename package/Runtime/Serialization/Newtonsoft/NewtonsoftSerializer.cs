using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Needle.Timeline.Serialization
{
	public class NewtonsoftSerializer : ISerializer
	{
		private static JsonSerializerSettings _settings;
		public static JsonSerializerSettings Settings
		{
			get
			{
				return _settings ??= new JsonSerializerSettings()
				{
					Converters = new List<JsonConverter>()
					{
						new Vec2Conv(),
						new Vec3Conv(),
						new Vec4Conv(),
						new ColConv(),
						
						new KeyframeConverter(),
						// new CustomClipConverter(),
						// new EasingConverter()
					}, 
					TypeNameHandling = TypeNameHandling.All,
					ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
					// PreserveReferencesHandling = PreserveReferencesHandling.Objects
				};
			}
		}

		public bool Indented = false;
		
		public object Serialize(object obj)
		{
			try
			{
				return JsonConvert.SerializeObject(obj, Indented ? Formatting.Indented : Formatting.None, Settings);
			}
			catch (JsonSerializationException ser)
			{
				Debug.LogException(ser);
				Debug.LogError(obj, obj as Object);
			}
			return null;
		}

		public T Deserialize<T>(object value)
		{
			return JsonConvert.DeserializeObject<T>((string)value, Settings);
		}

		public object Deserialize(object value, Type type)
		{
			return JsonConvert.DeserializeObject((string)value, type, Settings);
		}
	}
}
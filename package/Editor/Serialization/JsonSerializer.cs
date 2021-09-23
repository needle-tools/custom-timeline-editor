using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Needle.Timeline.Serialization
{
	public class JsonSerializer : ISerializer
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
						new Vec4Conv()
					}
				};
			}
		}
		
		public object Serialize(object obj)
		{
			return JsonConvert.SerializeObject(obj, Settings);
		}

		public object Deserialize(Type type, object value)
		{
			return JsonConvert.DeserializeObject((string)value, type, Settings);
		}
	}
}
#nullable enable

using System;
using Newtonsoft.Json;

namespace Needle.Timeline.Serialization
{
	public class KeyframeConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType.IsInterface && typeof(ICustomKeyframe).IsAssignableFrom(objectType);
		} 
		
		public override void WriteJson(JsonWriter writer, object? value, Newtonsoft.Json.JsonSerializer serializer)
		{
			serializer.Serialize(writer, value);
		}

		public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, Newtonsoft.Json.JsonSerializer serializer)
		{
			var type = typeof(CustomKeyframe<>).MakeGenericType(objectType.GenericTypeArguments);
			return serializer.Deserialize(reader, type); 
		}
	}
}
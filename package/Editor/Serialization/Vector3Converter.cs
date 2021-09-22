using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace Needle.Timeline.Serialization
{
	[JsonConverter(typeof(Vector3))]
	public class Vector3Converter : JsonConverter<Vector3>
	{
		public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
		{
			writer.WriteValue(value.x);
			writer.WriteValue(value.y);
			writer.WriteValue(value.z);
		}

		public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			existingValue.x = ReadAsFloat(reader).GetValueOrDefault();
			return existingValue;
		}
		

		public static float? ReadAsFloat(JsonReader reader)
		{
			// https://github.com/jilleJr/Newtonsoft.Json-for-Unity.Converters/issues/46

			var str = reader.ReadAsString();

			if (string.IsNullOrEmpty(str))
			{
				return null;
			}
			else if (float.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var valueParsed))
			{
				return valueParsed;
			}
			else
			{
				return 0f;
			}
		}
	}
}
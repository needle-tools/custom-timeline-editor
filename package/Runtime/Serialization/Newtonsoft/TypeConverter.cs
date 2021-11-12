#nullable enable

using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Needle.Timeline.Serialization
{
	public class KeyframeConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType.IsInterface && typeof(ICustomKeyframe).IsAssignableFrom(objectType);
		} 
		
		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, value);
		}

		public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			var type = typeof(CustomKeyframe<>).MakeGenericType(objectType.GenericTypeArguments);
			return serializer.Deserialize(reader, type); 
		} 
	}
	
	// public class CustomClipConverter : JsonConverter
	// {
	// 	public override bool CanConvert(Type objectType)
	// 	{
	// 		if(typeof(ICustomClip).IsAssignableFrom(objectType)) Debug.Log(objectType);
	// 		return typeof(ICustomClip).IsAssignableFrom(objectType);
	// 	} 
	// 	
	// 	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
	// 	{
	// 		Debug.Log(value);
	// 		serializer.Serialize(writer, value, value?.GetType());
	// 	}
	//
	// 	public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	// 	{ 
	// 		var type = typeof(CustomAnimationCurve<>).MakeGenericType(objectType.GenericTypeArguments);
	// 		return serializer.Deserialize(reader, type); 
	// 	}
	// }
	
	public class EasingConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			// return false;
			return typeof(ICurveEasing).IsAssignableFrom(objectType); 
		}

		public override bool CanWrite => false;
		public override bool CanRead => false;

		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{ 
			Debug.Log(value);
			writer.WriteStartObject(); ;
			var obj = new JObject();
			obj.Add("$Type", value?.GetType().ToString() ?? "NULL");
			// serializer.Serialize(writer, value);  
		}

		public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			// var type = typeof(CustomKeyframe<>).MakeGenericType(objectType.GenericTypeArguments);
			// return serializer.Deserialize(reader, type); 
			// return serializer.Deserialize(reader, objectType); 
			return null;
		}
	}
}
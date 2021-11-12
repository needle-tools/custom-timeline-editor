#nullable enable
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Needle.Timeline.Serialization
{
	public class NewtonsoftSerializer : ISerializer
	{
		private static JsonSerializerSettings? _settings;
		private static JsonSerializerSettings Settings
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
					Error = OnSerializationError,
					SerializationBinder = new SerializationBinderWithRecoveryStrategy()
					// ContractResolver = new Resolver(),
					// PreserveReferencesHandling = PreserveReferencesHandling.Objects
				};
			}
		}

		private static void OnSerializationError(object sender, ErrorEventArgs e)
		{
			// e.ErrorContext.Handled = true;
		}

		public bool Indented = false;
		
		public object? Serialize(object obj)
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
			return JsonConvert.DeserializeObject<T>((string)value, Settings)!;
		}

		public object? Deserialize(object value, Type type)
		{
			return JsonConvert.DeserializeObject((string)value, type, Settings);
		}
	}
}
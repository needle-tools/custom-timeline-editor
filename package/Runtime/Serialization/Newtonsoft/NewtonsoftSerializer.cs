#nullable enable
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Needle.Timeline.Serialization
{
	public class NewtonsoftSerializer : ISerializer
	{
		private JsonSerializerSettings? _settings;
		public JsonSerializerSettings Settings
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
					SerializationBinder = new SerializationBinderWithRecovery()
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
				if(obj is ISerializationCallbackReceiver res) res.OnBeforeSerialize();
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
			var res = Deserialize(value, typeof(T));
			if(res != null)
				return (T)res;
			return default!; 
		}

		public object? Deserialize(object value, Type type)
		{
			var res = JsonConvert.DeserializeObject((string)value, type, Settings);
			if(res is ISerializationCallbackReceiver cb)
				cb.OnAfterDeserialize();
			return res;
		}
	}
}
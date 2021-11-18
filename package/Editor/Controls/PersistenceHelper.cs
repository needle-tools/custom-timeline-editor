#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Needle.Timeline.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Needle.Timeline
{
	internal class PersistenceHelper
	{
		public bool TryGetPreviousState<T>(string key, out T value)
		{
			var valueKey = new Key(key, typeof(T).Name, null);
			if (TryGetPreviousState(valueKey, out var res))
			{
				if (res is T b)
				{
					value = b;
					return true;
				}
			}

			value = default!;
			return false;
		}

		public void OnStateChanged<T>(string key, T newValue)
		{
			if (newValue == null) return;
			EnsureStateLoaded();
			if (state == null) return;
			var valueKey = new Key(key, typeof(T).Name, null);
			state[valueKey] = newValue;
		}

		public bool TryGetPreviousState(FieldInfo field, out object value)
		{
			var valueKey = new Key(field);
			if (TryGetPreviousState(valueKey, out value))
			{
				if (field.FieldType.IsInstanceOfType(value))
					return true;

				try
				{
					value = value.Cast(field.FieldType);
					if (value != null)
						return true;
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			}
			value = null!;
			return false;
		}

		public void OnStateChanged(FieldInfo field, object newValue)
		{
			EnsureStateLoaded();
			if (state == null) return;
			var key = new Key(field);
			state[key] = newValue;
		}

		public PersistenceHelper(string id)
		{
			this.id = id;
			_instances.Add(this);
		}

		private bool TryGetPreviousState(Key valueKey, out object value)
		{
			EnsureStateLoaded();
			if (state != null && state.TryGetValue(valueKey, out value))
			{
				if (value != null)
					return true;
			}
			value = null!;
			return false;
		}

		private void EnsureStateLoaded()
		{
			if (state == null)
			{
				var path = savePath;
				var dir = Path.GetDirectoryName(path);
				if (!Directory.Exists(dir) || !File.Exists(path))
				{
					state = new Dictionary<Key, object>();
					return;
				}
				try
				{
					var content = File.ReadAllText(path);
					if (string.IsNullOrEmpty(content)) state = new Dictionary<Key, object>();
					else
					{
						state = serializer.Deserialize<KeyValuePair<Key, object>[]>(content).ToDictionary(k => k.Key, e => e.Value);
						PostProcess(state);
					}
				}
				catch (Exception e)
				{
					Debug.LogException(e);
					state = new Dictionary<Key, object>();
				}
			}
		}

		private void SaveState(bool force = false)
		{
			if (!force && (state == null || state.Count <= 0)) return;
			var path = savePath;
			var dir = Path.GetDirectoryName(path);
			if (string.IsNullOrEmpty(dir)) return;
			if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
			var ser = (string)serializer.Serialize(state?.ToArray());
			File.WriteAllText(path, ser);
		}


		private readonly ISerializer serializer = new NewtonsoftSerializer()
		{
			Settings =
			{
				TypeNameHandling = TypeNameHandling.Auto,
				NullValueHandling = NullValueHandling.Ignore
			}
		};

		private Dictionary<Key, object>? state = null;
		private readonly string id;
		private string savePath => Application.dataPath + "/../UserSettings/Timeline/" + id + ".json";

		private struct Key
		{
			[JsonProperty("p0")] public readonly string? part0;
			[JsonProperty("p1")] public readonly string? part1;
			[JsonProperty("p2")] public readonly string? part2;

			public Key(string p0, string p1, string? p2)
			{
				part0 = p0;
				part1 = p1;
				part2 = p2;
			}

			public Key(string name, Type type) : this(name, type.Name, "")
			{
			}

			public Key(FieldInfo field) : this(field.Name, field.FieldType.Name, field.DeclaringType?.Name)
			{
			}
		}

		private static readonly List<PersistenceHelper> _instances = new List<PersistenceHelper>();

		[InitializeOnLoadMethod]
		private static void Init()
		{
			AssemblyReloadEvents.beforeAssemblyReload += OnBeforeReload;
		}

		private static void OnBeforeReload()
		{
			foreach (var i in _instances)
			{
				i.SaveState();
			}
		}


		private static readonly List<(Key key, object? newValue)> postProcessed = new List<(Key, object?)>();

		private static void PostProcess(Dictionary<Key, object> values)
		{
			postProcessed.Clear();
			foreach (var val in values)
			{
				try
				{
					if (val.Value is JObject obj)
					{
						if (TryGetColor(obj, out var col))
						{
							postProcessed.Add((val.Key, col));
							continue;
						}
						if (TryGetVector3(obj, out var vec))
						{
							postProcessed.Add((val.Key, vec));
							continue;
						}
						
						postProcessed.Add((val.Key, null));
					}
				}
				catch (Exception e)
				{
					Debug.LogException(e);
					postProcessed.Add((val.Key, null));
				}
			}
			foreach (var pp in postProcessed)
			{
				if (pp.newValue == null)
					values.Remove(pp.key);
				else
					values[pp.key] = pp.newValue;
			}
			postProcessed.Clear();
		}

		private static bool TryGetColor(JObject obj, out Color col)
		{
			if (obj.HasValues)
			{
				// color values are not deserialized to color object
				// not sure how to set it up to do that automatically
				var r = obj["r"];
				var g = obj["g"];
				var b = obj["b"];
				var a = obj["a"];
				if (r != null && g != null && b != null)
				{
					col = new Color(r.Value<float>(), g.Value<float>(), b.Value<float>());
					if (a != null) col.a = a.Value<float>();
					return true;
				}
			}
			col = default;
			return false;
		}

		private static bool TryGetVector3(JObject obj, out object vec)
		{
			if (obj.HasValues && obj.Count >= 2)
			{
				var x = obj["x"];
				var y = obj["y"];
				var z = obj["z"];
				var w = obj["w"];
				if (x != null && y != null)
				{
					if (obj.Count == 2)
					{
						vec = new Vector2(x.Value<float>(), y.Value<float>());
						return true;
					}
					if (obj.Count == 3 && z != null)
					{ 
						vec = new Vector3(x.Value<float>(), y.Value<float>(), z.Value<float>());
						return true;
					}
					if (obj.Count == 4 && z != null && w != null)
					{
						vec = new Vector4(x.Value<float>(), y.Value<float>(), z.Value<float>(), w.Value<float>());
						return true;
					}
				}
			}
			vec = null;
			return false;
		}
	}
}
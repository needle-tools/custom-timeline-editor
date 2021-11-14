using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Needle.Timeline.AssetBinding
{
	[InitializeOnLoad]
	public static class AssetBinder
	{
		static AssetBinder()
		{
			cache.Clear();
			var fields = TypeCache.GetFieldsWithAttribute<BindAsset>();
			foreach (var field in fields)
			{
				EnsureBinding(field.DeclaringType);
			}
		}

		private class CacheEntry
		{
			public Type Type;
			public readonly List<(FieldInfo field, BindAsset binding)> MarkedFields = new List<(FieldInfo field, BindAsset binding)>();
			public readonly List<Object> BoundValues = new List<Object>();

			public CacheEntry(Type type)
			{
				Type = type;
			}
		}

		private static readonly List<CacheEntry> cache = new List<CacheEntry>();

		public static void EnsureBinding(Type type)
		{
			var entry = cache.FirstOrDefault(x => x.Type == type);
			if (entry != null)
			{
				for (var index = 0; index < entry.BoundValues.Count; index++)
				{
					var val = entry.BoundValues[index];
					if (!val && val != BindAsset.CouldNotLoadMarker)
					{
						var marked = entry.MarkedFields[index];
						var binding = marked.binding;
						var field = marked.field;
						var res = binding.Apply(field);
						entry.MarkedFields[index] = (field, binding);
						entry.BoundValues[index] = res;
					}
				}
			}
			else 
			{
				entry = new CacheEntry(type);
				cache.Add(entry);
				foreach (var field in type.GetFields(BindingFlags.Default | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
				{
					var binding = field.GetCustomAttribute<BindAsset>();
					if (binding == null) continue; 
					var res = binding.Apply(field);
					entry.MarkedFields.Add((field, binding));
					entry.BoundValues.Add(res); 
				} 
			}
		}
	}
}
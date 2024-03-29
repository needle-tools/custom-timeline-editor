﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Needle.Timeline
{
	public class ImplementorsRegistry<T> : IRegistry where T : class
	{
		public bool TryGetInstance(Predicate<T> test, out T instance, IList<IArgument>? args = null)
		{
			EnsureCached();

			if (instances == null)
			{
				instances = new T[cache!.Length];
				for (var index = 0; index < cache.Length; index++)
				{
					var t = cache[index];
					if(t.TryCreateInstance(args, out var i))
						instances[index] = (T)i; 
				}
			}
			foreach (var inst in instances)
			{
				var obj = inst; 
				if (test(obj))
				{
					instance = obj;
					return true;
				}
			}
			
			instance = default!;
			return false;
		}

		public bool TryGetNewInstance<TInstanceType>(out TInstanceType instance, IList<IArgument>? args = null) where TInstanceType : T
		{
			if (TryGetNewInstance(typeof(TInstanceType), out var i, args))
			{
				instance = (TInstanceType)i;
				return true;
			}
			instance = default!;
			return false;
		}

		public bool TryGetNewInstance(Type type, out object instance, IList<IArgument>? args = null)
		{
			if (TryFind(type.IsAssignableFrom, out var t))
			{
				if (t.TryCreateInstance(args, out var i))
				{
					instance = i;
					return instance != null;
				}
			}
			instance = default!;
			return false;
		}
		
		public bool TryFind(Predicate<Type> test, out Type match)
		{
			EnsureCached();
			foreach (var ch in cache!)
			{
				if (test(ch))
				{
					match = ch;
					return true;
				}
			}

			match = null!;
			return false;
		}

		public IList<Type> GetAll()
		{
			EnsureCached();
			return cache ?? Array.Empty<Type>();
		}

		private Type[]? cache;
		private T[]? instances;

		private void EnsureCached()
		{
			if (cache == null)
			{
				var list = new List<Type>();
				foreach (var t in RuntimeTypeCache.GetTypesDerivedFrom<T>())
				{
					if (t.IsAbstract || t.IsInterface) continue;
					if (t.GetCustomAttribute<RegistryIgnoreAttribute>() != null) continue;
					list.Add(t);
				}

				cache = list.OrderByDescending(e =>
					{
						var prio = 0;
						var attributes = e.GetCustomAttributes(true);
						foreach (var att in attributes)
						{
							if (att is IPriority p)
								prio += p.Priority;
						}
						return prio;
					})
					.ToArray();
			}
		}
	}
}
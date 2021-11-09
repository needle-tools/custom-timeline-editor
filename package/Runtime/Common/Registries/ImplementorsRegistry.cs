﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace Needle.Timeline
{
	public class ImplementorsRegistry<T> : IRegistry<T> where T : class
	{
		public bool TryGetInstance(Predicate<T> test, out T instance)
		{
			EnsureCached();

			if (instances == null)
			{
				instances = new T[cache!.Length];
				for (var index = 0; index < cache.Length; index++)
				{
					var t = cache[index];
					var i = (T)Activator.CreateInstance(t);
					instances[index] = i;
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
		
		public bool TryGetNewInstance<TInstanceType>(out TInstanceType instance) where TInstanceType : T
		{
			if (TryFind(e => typeof(TInstanceType).IsAssignableFrom(e), out var type))
			{
				instance = (TInstanceType)Activator.CreateInstance(type);
				return instance != null;
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
			return cache!;
		}

		private Type[]? cache;
		private T[]? instances;

		private void EnsureCached()
		{
			if (cache == null)
			{
				var list = new List<Type>();
				foreach (var t in RuntimeTypeCache.GetTypesDerivingFrom<T>())
				{
					if (t.IsAbstract || t.IsInterface) continue;
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
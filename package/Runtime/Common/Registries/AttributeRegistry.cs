#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Needle.Timeline
{
	public class AttributeRegistry<T> : IRegistry<T> where T : Attribute
	{
		public bool TryFind(Predicate<Type> test, out Type match)
		{
			var type = typeof(T);
			if (!typeof(Attribute).IsAssignableFrom(type))
			{
				match = default!;
				return false;
			}
			
			EnsureCached();

			foreach (var e in cache!)
			{
				if (test(e))
				{
					match = e;
					return true;
				}
			}
			match = default!;
			return false;
		}

		public IList<Type> GetAll()
		{
			EnsureCached();
			return cache!;
		}

		private Type[]? cache;

		private void EnsureCached()
		{
			if (cache == null)
			{
				var list = new List<Type>();
				foreach (var t in RuntimeTypeCache.Types)
				{
					var att = t.GetCustomAttribute<T>();
					if (att != null) list.Add(t);
				}

				cache = list.OrderByDescending(e =>
					{
						var prio = 0;
						var attributes = e.GetCustomAttributes();
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
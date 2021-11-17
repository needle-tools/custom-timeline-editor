using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Needle.Timeline
{
	internal static class BindingsCache
	{
		internal static void Register(Type owner, IViewFieldBinding view)
		{
			if (owner == null) throw new Exception("Missing type");
			if (!cache.ContainsKey(owner))
			{
				cache[owner] = new List<IViewFieldBinding>();
			}
			var list = cache[owner];
			if(!list.Any(e => e.Matches(view.ValueType)))
			   list.Add(view);
		}

		private static readonly Dictionary<Type, List<IViewFieldBinding>> cache = new Dictionary<Type, List<IViewFieldBinding>>();

		internal static bool TryGetFromCache(FieldInfo field, out IViewFieldBinding controller)
		{
			var type = field.DeclaringType;
			if (type != null)
			{
				if (cache.TryGetValue(type, out var list))
				{
					controller = list.FirstOrDefault(e => e.Matches(field));
					return controller != null;
				}
			}
			controller = null;
			return false;
		}

	}
}
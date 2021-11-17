using System.Collections.Generic;
using System.Reflection;

namespace Needle.Timeline
{
	internal static class BindingsCache
	{
		internal static void Register(IViewValueHandler view)
		{
			if (!cached.Contains(view))
				cached.Add(view);
		}

		internal static bool TryGetFromCache(FieldInfo field, out IViewValueHandler controller)
		{
			foreach (var ch in cached)
			{
				if (ch.Name == field.Name && ch.ValueType == field.FieldType)
				{
					controller = ch;
					return true;
				}
			}
			controller = null;
			return false;
		}

		private static readonly List<IViewValueHandler> cached = new List<IViewValueHandler>();
	}
}
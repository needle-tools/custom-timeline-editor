using System;
using System.Linq;

#nullable enable

namespace Needle.Timeline
{
	public class TypesSupportAttribute : Attribute
	{
		public int Priority;
		public Type? SupportedType;
		public Type[]? SupportedTypes;

		public bool Supports(Type type)
		{
			if (SupportedType != null) return SupportedType.IsAssignableFrom(type);
			return SupportedTypes?.Any(e => e.IsAssignableFrom(type)) ?? false;
		}
	}
}
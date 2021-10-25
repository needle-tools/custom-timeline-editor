using System;
using System.Linq;

#nullable enable

namespace Needle.Timeline
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class ShaderBridgeAttribute : Attribute
	{
		public int Priority;
		public readonly Type? SupportedType;
		public readonly Type[]? SupportedTypes;

		public ShaderBridgeAttribute(Type supportedType)
		{
			SupportedType = supportedType;
		}

		public ShaderBridgeAttribute(params Type[] supportedTypes)
		{
			this.SupportedTypes = supportedTypes;
		}

		public bool Supports(Type type)
		{
			if (SupportedType != null) return SupportedType.IsAssignableFrom(type);
			return SupportedTypes?.Any(e => e.IsAssignableFrom(type)) ?? false;
		}
	}
}
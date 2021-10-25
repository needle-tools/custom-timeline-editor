using System;
using System.Linq;

#nullable enable

namespace Needle.Timeline
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class ShaderBridgeAttribute : TypesSupportAttribute
	{
		public ShaderBridgeAttribute(Type supportedType)
		{
			SupportedType = supportedType;
		}

		public ShaderBridgeAttribute(params Type[] supportedTypes)
		{
			this.SupportedTypes = supportedTypes;
		}
	}
}
using System;

namespace Needle.Timeline
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class NoClone : Attribute
	{
		
	}
}
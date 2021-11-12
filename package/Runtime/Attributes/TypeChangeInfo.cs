using System;
using JetBrains.Annotations;
#nullable enable

namespace Needle.Timeline
{
	// MovedFrom?
	
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property)]
	public class RefactorInfo : Attribute
	{
		public readonly string? OldAssemblyName;
		public readonly string? OldName;

		public RefactorInfo(string? oldName, string? oldAssemblyName = null)
		{
			OldName = oldName;
			OldAssemblyName = oldAssemblyName;
		}
	}
}
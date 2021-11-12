using System;
using JetBrains.Annotations;
#nullable enable

namespace Needle.Timeline
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property)]
	public class RefactorInfo : Attribute
	{
		public readonly string? OldAssemblyName;
		public readonly string OldName;

		public RefactorInfo(string oldName)
		{
			OldName = oldName;
		}
	}
}
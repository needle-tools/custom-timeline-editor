using System;
using System.Collections.Generic;

namespace Needle.Timeline
{
	public readonly struct ModificationIdentifier
	{
		public readonly int Index;
		public readonly int MemberIndex;

		public ModificationIdentifier(int index, int memberIndex)
		{
			Index = index;
			this.MemberIndex = memberIndex;
		}
		
		public ModificationIdentifier(ModifyContext context) : this(context.Index, context.MemberIndex){}
	}

	public static class ModificationUtils
	{
		public static bool Contains(this IList<ModificationIdentifier> list, int index, int memberIndex)
		{
			for (var i = 0; i < list.Count; i++)
			{
				var e = list[i];
				if (e.Index == index && e.MemberIndex == memberIndex) return true;
			}
			return false;
		}
	}
}
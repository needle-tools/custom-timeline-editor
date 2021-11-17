using System;
using System.Collections.Generic;

namespace Needle.Timeline
{
	public readonly struct ModificationIdentifier
	{
		public readonly int Hash;
		public readonly int Index;
		public readonly int MemberIndex;
		public readonly float Weight;

		public static ModificationIdentifier Null { get; } = new ModificationIdentifier();

		public ModificationIdentifier(int hash, int index, int memberIndex, float weight)
		{
			this.Hash = hash;
			Index = index;
			this.MemberIndex = memberIndex;
			this.Weight = weight;
		}
		
		public ModificationIdentifier(ModifyContext context, float factor = 1) : this(context.TargetHash, context.Index, context.MemberIndex, factor){}
	}

	public static class ModificationUtils
	{
		public static bool Contains(this IList<ModificationIdentifier> list, int hash,  int index, int memberIndex, out ModificationIdentifier found)
		{
			for (var i = 0; i < list.Count; i++)
			{
				var e = list[i];
				if (e.Hash == hash && e.Index == index && e.MemberIndex == memberIndex)
				{
					found = e;
					return true;
				}
			}
			found = ModificationIdentifier.Null;
			return false;
		}
	}
}
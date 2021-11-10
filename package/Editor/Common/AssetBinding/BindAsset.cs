using System;
using System.ComponentModel;

namespace Needle.Timeline.AssetBinding
{
	[AttributeUsage(AttributeTargets.Field)]
	public class BindAsset : Attribute
	{
		public readonly string Guid;

		public BindAsset(string guid)
		{
			this.Guid = guid;
		}
	}
}
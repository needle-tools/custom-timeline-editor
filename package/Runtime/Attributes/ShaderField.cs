using System;

namespace Needle.Timeline
{
	public class ShaderField : Attribute
	{
		public readonly string Name;

		public ShaderField(string name)
		{
			Name = name;
		}
	}
}
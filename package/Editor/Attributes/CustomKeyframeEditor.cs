using System;

namespace Needle.Timeline
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class CustomKeyframeEditorAttribute : Attribute
	{
		public readonly Type Type;

		public CustomKeyframeEditorAttribute(Type type) => this.Type = type;
	}
}
using System;
using System.Text;

namespace Needle.Timeline
{
	public static class StringHelper
	{
		public static void GetTypeStringWithGenerics(Type type, StringBuilder builder, int level = 0)
		{
			var generics = type.GenericTypeArguments;
			var genericsIndex = type.Name.IndexOf('`');
			if (genericsIndex > 0)
				builder.Append(type.Name.Substring(0, genericsIndex));
			else
				builder.Append(type.Name);
			if (generics != null && generics.Length > 0)
			{
				builder.Append("<");
				var nextLevel = ++level;
				for (var index = 0; index < generics.Length; index++)
				{
					var nested = generics[index];
					GetTypeStringWithGenerics(nested, builder, nextLevel);
					if (index < generics.Length - 1)
						builder.Append(", ");
				}
				builder.Append(">");
			}
		}

		public static void GetGenericsString(Type type, StringBuilder builder, int level = 0)
		{
			var generics = type.GenericTypeArguments;
			if (generics != null && generics.Length > 0)
			{
				if (level > 0)
					builder.Append("<");
				var nextLevel = level + 1;
				for (var index = 0; index < generics.Length; index++)
				{
					var nested = generics[index];
					GetTypeStringWithGenerics(nested, builder, nextLevel);
					if (index < generics.Length - 1)
						builder.Append(", ");
				}
				if (level > 0)
					builder.Append(">");
			}
		}
	}
}
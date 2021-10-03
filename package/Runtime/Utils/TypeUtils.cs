using System;
using System.Linq;
using System.Reflection;

namespace Needle.Timeline
{
	public static class TypeUtils
	{
		private static readonly Type[] defaultConstructorParameters = Type.EmptyTypes;
		
		public static ConstructorInfo GetDefaultConstructor(this Type t)
		{
			var c = t.GetConstructor(defaultConstructorParameters);
			return c;
		}

		public static PropertyInfo GetIndexer(this Type t)
		{
			return t.GetProperties().FirstOrDefault(p => p.GetIndexParameters().Length != 0);
		}
	}
}
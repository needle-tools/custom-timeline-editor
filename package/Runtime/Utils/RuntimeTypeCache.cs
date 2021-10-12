using System;
using System.Collections.Generic;
using System.Reflection;

namespace Needle.Timeline
{
	public static class RuntimeTypeCache
	{
		private static Assembly[] _assemblies;
		private static List<Type> _types;

		public static IReadOnlyList<Assembly> Assemblies => _assemblies ??= AppDomain.CurrentDomain.GetAssemblies();

		public static IReadOnlyList<Type> Types
		{
			get
			{
				if (_types == null)
				{
					_types = new List<Type>();
					foreach (var asm in Assemblies)
					{
						foreach (var t in asm.GetTypes())
							_types.Add(t);
					}
				}
				return _types;
			}
		}

		public static IEnumerable<Type> TypesDerivingFrom<T>()
		{
			foreach (var t in Types)
			{
				if (typeof(T).IsAssignableFrom(t)) yield return t;
			}
		}
	}
}
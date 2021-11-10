using System;
using System.Collections.Generic;
using System.Linq;

namespace Needle.Timeline
{
	public static class ArgumentExtensions
	{
		public static bool TryFind(this IList<IArgument> args, string name, Type type, out IArgument arg)
		{
			for (var index = 0; index < args.Count; index++)
			{
				var a = args[index];
				if (a.Match(name, type))
				{
					arg = a;
					return true;
				}
			}
			arg = default;
			return false;
		}

		public static object[] ToArgumentArray(this IEnumerable<IArgument> args)
		{
			return args.Select(a => a.Value).ToArray();
		}


		public static bool TryCreateInstance<T>(IList<IArgument>? args, out T instance)
		{
			if (TryCreateInstance(typeof(T), args, out var i))
			{
				instance = (T)i;
				return true;
			}
			instance = default!;
			return false;
		}

		public static bool TryCreateInstance(this Type type, IList<IArgument>? args, out object instance)
		{
			if (type.IsAbstract || type.IsInterface)
			{
				throw new Exception("Invalid type (abstract or interface?): " + type.FullName);
			}

			var hasDefaultConstructor = false;
			if (args != null && args.Count > 0)
			{
				var constructors = type.GetConstructors();
				var matchingArgs = new List<IArgument>();
				foreach (var c in constructors)
				{
					matchingArgs.Clear();
					var pars = c.GetParameters();
					hasDefaultConstructor |= pars.Length <= 0;
					var failed = false;
					foreach (var par in pars)
					{
						if (failed) break;
						if (args.TryFind(par.Name, par.ParameterType, out var arg))
							matchingArgs.Add(arg);
						else failed = true;
					}
					if (!failed) 
					{
						instance = Activator.CreateInstance(type, matchingArgs.ToArgumentArray());
						return true;
					}
				}
			}
			// if we have no args assume we have a default constructor
			else hasDefaultConstructor = true;

			if (hasDefaultConstructor)
			{
				instance = Activator.CreateInstance(type); 
				return true;
			}

			instance = null;
			return false;
		}
	}
}
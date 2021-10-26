using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Needle.Timeline
{
	internal static class TypeCastHelper
	{
		// via https://newbedev.com/casting-a-variable-using-a-type-variable
		internal static object Cast(this object o, Type t)
		{
			if (o == null) return null;
			var type = o.GetType();
			if (type == t) return o;
			return GetCastDelegate(type, t).Invoke(o);
		}
		
		private static Func<object, object> MakeCastDelegate(Type from, Type to)
		{
			var p = Expression.Parameter(typeof(object)); //do not inline
			return Expression.Lambda<Func<object, object>>(
				Expression.Convert(Expression.ConvertChecked(Expression.Convert(p, from), to), typeof(object)),
				p).Compile();
		}

		private static readonly Dictionary<Tuple<Type, Type>, Func<object, object>> 
			castCache = new Dictionary<Tuple<Type, Type>, Func<object, object>>();

		private static Func<object, object> GetCastDelegate(Type from, Type to)
		{
			lock (castCache)
			{
				var key = new Tuple<Type, Type>(from, to);
				Func<object, object> cast_delegate;
				if (castCache.TryGetValue(key, out cast_delegate)) return cast_delegate;
				cast_delegate = MakeCastDelegate(@from, to);
				castCache.Add(key, cast_delegate);
				return cast_delegate;
			}
		}
	}
}
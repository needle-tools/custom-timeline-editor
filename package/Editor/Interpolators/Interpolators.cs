using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Needle.Timeline
{
	public static class Interpolators
	{

		public static bool TryFindInterpolator(AnimateAttribute attribute, Type type, out IInterpolator interpolator)
		{
			var searchType = typeof(IInterpolator<>).MakeGenericType(type);
			foreach (var t in TypeCache.GetTypesDerivedFrom(searchType))
			{
				if (t.IsAbstract || t.IsInterface) continue;
				if (typeof(ICustomClip).IsAssignableFrom(t)) continue;
				try
				{
					interpolator = Activator.CreateInstance(t) as IInterpolator;
					if (interpolator != null) return true;
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			}


			foreach (var t in TypeCache.GetTypesDerivedFrom(typeof(IInterpolator)))
			{
				if (t.IsAbstract || t.IsInterface || t.ContainsGenericParameters) continue;
				if (!interpolatorsCache.ContainsKey(t))
				{
					var i = Activator.CreateInstance(t) as IInterpolator;
					interpolatorsCache.Add(t, i);
				}
				var instance = interpolatorsCache[t];
				if (instance?.CanInterpolate(type) == true)
				{
					interpolator = Activator.CreateInstance(t) as IInterpolator;
					return true;
				};
			}

			interpolator = null;
			return false;
		}

		private static Dictionary<Type, IInterpolator> interpolatorsCache = new Dictionary<Type, IInterpolator>();
	}
}
using System;
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
			interpolator = null;
			return false;
		}
	}
}
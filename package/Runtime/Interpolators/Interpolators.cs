using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Needle.Timeline
{
	public static class Interpolators
	{
		public static bool TryFindInterpolator(AnimateAttribute attribute, Type memberType, out IInterpolator interpolator)
		{
			if (attribute.AllowInterpolation == false)
			{
				interpolator = new NoInterpolator();
				return true;
			}

			var genericInterpolatorType = typeof(IInterpolator<>).MakeGenericType(memberType);
			int Ordering(Type t) => t.GetCustomAttribute<Priority>()?.Rating ?? 0;

			foreach (var t in TypeCache.GetTypesDerivedFrom(genericInterpolatorType).OrderByDescending(Ordering))
			{
				if (t.IsAbstract || t.IsInterface) continue;
				if (t.GetCustomAttribute<NoAutoSelect>() != null) continue;
				// ignore custom clips implementing interpolator interface
				if (typeof(ICustomClip).IsAssignableFrom(t)) continue;
				if (attribute.Interpolator != null && t != attribute.Interpolator) continue;
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


			foreach (var type in TypeCache.GetTypesDerivedFrom(typeof(IInterpolator)).OrderByDescending(Ordering))
			{
				if (type.IsAbstract || type.IsInterface) continue;
				if (typeof(ICustomClip).IsAssignableFrom(type)) continue;
				
				if (attribute.Interpolator != null)
				{
					// if the field has a specific interpolator defined and this is not it
					if (type != attribute.Interpolator)
						continue;
				}
				// if the interpolator is marked as not being used from here
				else if (type.GetCustomAttribute<NoAutoSelect>() != null) continue;

				var interpolatorType = type;
				if (interpolatorType.ContainsGenericParameters)
				{
					interpolatorType = interpolatorType.MakeGenericType(memberType);
				}
				
				if (!interpolatorsCache.ContainsKey(interpolatorType))
				{
					try
					{
						if (interpolatorType.GetDefaultConstructor() != null) 
						{
							if (interpolatorType.ContainsGenericParameters)
							{
								IInterpolator Create()
								{
									var t = type.MakeGenericType(memberType);
									return Activator.CreateInstance(t) as IInterpolator;
								}
								interpolatorsCache.Add(interpolatorType, (Create(), Create));
							}
							else
							{
								IInterpolator Create()
								{
									return Activator.CreateInstance(interpolatorType) as IInterpolator;
								}
								interpolatorsCache.Add(interpolatorType, (Create(), Create));
							}
						}
						else
						{
							// TODO: implement case where default constructor is not existing e.g. computebufferinterpolator????
							interpolatorsCache.Add(interpolatorType, (null, null));
						}
					}
					catch (MissingMemberException m)
					{
						interpolatorsCache.Add(interpolatorType, (null, null));
						Debug.LogException(m);
					}
				}

				// TODO: check if script implements method for collection interpolation? 
				// e.g. Guide Interpolate(Guide v0, Guide v1, float t);

				var kvp = interpolatorsCache[interpolatorType];
				var instance = kvp.instance;
				if (instance?.CanInterpolate(memberType) == true)
				{
					interpolator = kvp.create();
					return true; 
				}
			}

			interpolator = null;
			return false;
		}

		private static readonly Dictionary<Type, (IInterpolator instance, Func<IInterpolator> create)> interpolatorsCache 
			= new Dictionary<Type, (IInterpolator instance, Func<IInterpolator> create)>();
	}
}
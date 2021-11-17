using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Needle.Timeline
{
	public static class InterpolatorBuilder
	{
		public static bool TryFindInterpolatable(Type type, out IInterpolatable interpolatable, bool allowReflective = true)
		{
			var genericType = typeof(IInterpolatable<>).MakeGenericType(type);
			int Ordering(MemberInfo t) => t.GetCustomAttribute<Priority>()?.Rating ?? 0;
			
			// first check if we have a concrete implementation
			foreach (var t in RuntimeTypeCache.GetTypesDerivedFrom(genericType).OrderByDescending(Ordering))
			{
				if (t.IsAbstract || t.IsInterface) continue;
				if (t.GetCustomAttribute<NoAutoSelect>() != null) continue;  
				try
				{
					interpolatable = Activator.CreateInstance(t) as IInterpolatable;
					if (interpolatable != null) return true;
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			}


			foreach (var t in RuntimeTypeCache.GetTypesDerivedFrom(typeof(IInterpolatable)).OrderByDescending(Ordering))
			{ 
				var sup = t.GetCustomAttribute<InterpolatableAttribute>();
				if (sup == null) continue;
				if (sup.SupportedTypes.Any(x => x.IsAssignableFrom(type)))
				{
					try
					{
						interpolatable = Activator.CreateInstance(t) as IInterpolatable;
						if (interpolatable != null) return true;
					}
					catch (Exception e)
					{
						Debug.LogException(e);
					}
				}
			}
			

			// otherwise fallback to reflective interpolator if possible
			if (allowReflective && ReflectiveInterpolatable.TryCreate(type, out var ri))
			{
				interpolatable = ri;
				return interpolatable != null;
			}
			
			interpolatable = null;
			return false;
		}
		
		public static bool TryFindInterpolator(Type memberType, out IInterpolator interpolator, 
			Type expectedType = null)
		{
			var genericInterpolatorType = typeof(IInterpolator<>).MakeGenericType(memberType);
			int Ordering(Type t) => t.GetCustomAttribute<Priority>()?.Rating ?? 0;

			foreach (var t in RuntimeTypeCache.GetTypesDerivedFrom(genericInterpolatorType).OrderByDescending(Ordering))
			{
				if (t.IsAbstract || t.IsInterface) continue;
				if (t.GetCustomAttribute<NoAutoSelect>() != null) continue;
				// ignore custom clips implementing interpolator interface
				if (typeof(ICustomClip).IsAssignableFrom(t)) continue;
				if (expectedType != null && t != expectedType) continue;
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


			foreach (var type in RuntimeTypeCache.GetTypesDerivedFrom(typeof(IInterpolator)).OrderByDescending(Ordering))
			{
				if (type.IsAbstract || type.IsInterface) continue;
				if (typeof(ICustomClip).IsAssignableFrom(type)) continue;
				
				if (expectedType != null)
				{
					// if the field has a specific interpolator defined and this is not it
					if (type != expectedType)
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
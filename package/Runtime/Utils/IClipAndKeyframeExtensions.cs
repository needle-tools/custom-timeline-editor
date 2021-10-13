using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Needle.Timeline
{
	public static class IClipAndKeyframeExtensions
	{
		internal static Type TryRetrieveKeyframeContentType(this ICustomKeyframe kf)
		{
			var valueType = kf.value?.GetType() ?? kf.AcceptedTypes()?.FirstOrDefault();
			if (valueType == null) return null;
			if (typeof(ICollection).IsAssignableFrom(valueType))
			{
				if (valueType.IsGenericType)
				{
					var content = valueType.GetGenericArguments().FirstOrDefault();
					return content;
				}
				
				Debug.Log("Unhandled: " + valueType);
			}
			
			return valueType;
		}
		
		public static IEnumerable<FieldInfo> EnumerateFields(this ICustomClip clip)
		{
			foreach (var type in clip.SupportedTypes)
			{
				foreach (var field in type.EnumerateFields())
					yield return field;
			}
		}
		
		public static IEnumerable<FieldInfo> EnumerateFields(this ICustomKeyframe kf)
		{
			foreach (var type in kf.AcceptedTypes())
			{
				foreach (var field in type.EnumerateFields())
					yield return field;
			}
		}

		internal static IEnumerable<FieldInfo> EnumerateFields(this Type type,
			BindingFlags flags = BindingFlags.Default | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)

		{
			if (typeof(ICollection).IsAssignableFrom(type))
			{
				if (type.IsGenericType)
				{
					var content = type.GetGenericArguments().FirstOrDefault();
					if (content != null)
					{
						foreach (var field in content.GetFields(flags))
						{
							yield return field;
						}
					}
				}
			}
			else
			{
				foreach (var field in type.GetFields(flags))
				{
					yield return field;
				}
			}
		}
	}
}
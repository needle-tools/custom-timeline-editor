using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Needle.Timeline
{
	public static class IClipAndKeyframeExtensions
	{
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

		internal static IEnumerable<FieldInfo> EnumerateFields(this Type type)
		{
			if (typeof(ICollection).IsAssignableFrom(type))
			{
				if (type.IsGenericType) 
				{
					var content = type.GetGenericArguments().FirstOrDefault();
					if (content != null)
					{
						foreach (var field in content.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
						{
							yield return field;
						}
					}
				}
			}
		}
	}
}
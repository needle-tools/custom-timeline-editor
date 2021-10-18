#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Needle.Timeline
{
	public static class ClipAndKeyframeExtensions
	{
		public static ICustomKeyframe? AddKeyframe(this ICustomClip clip, float time, object? value = null)
		{
			if (clip.GetType().IsGenericType)
			{
				var clipType = clip.GetType().GetGenericArguments().FirstOrDefault();
				var keyframeType = typeof(CustomKeyframe<>).MakeGenericType(clipType);
				if (Activator.CreateInstance(keyframeType) is ICustomKeyframe kf)
				{
					kf.time = time;
					kf.value = value;
					Debug.Log("create Keyframe at " + time);
					CustomUndo.Register(new CreateKeyframe(kf, clip));
					return kf;
				}
			}
			return null;
		}
		
		internal static Type? TryRetrieveKeyframeContentType(this ICustomKeyframe kf)
		{
			var valueType = kf.value?.GetType() ?? kf.AcceptedTypes()?.FirstOrDefault();
			if (valueType == null) return null;
			if (typeof(ICollection).IsAssignableFrom(valueType))
			{
				if (valueType.IsGenericType)
				{
					var content = valueType.GetGenericArguments().First();
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
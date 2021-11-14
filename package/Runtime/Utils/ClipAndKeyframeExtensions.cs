#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.Profiling;
using UnityEngine;

namespace Needle.Timeline
{
	public static class ClipAndKeyframeExtensions
	{
		
		public static ICustomKeyframe? AddKeyframeWithUndo(this ICustomClip clip, float time, object? value = null)
		{
			if (clip.GetType().IsGenericType)
			{
				var clipType = clip.GetType().GetGenericArguments().FirstOrDefault();
				Debug.Log(clipType + " == " + clip.GetType().GetElementType());
				// Debug.Assert(clipType == clip.GetType().GetElementType());
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

		public static void AddWeightChange(this ICustomKeyframe current, ICustomKeyframe next, float change)
		{
			current.easeOutWeight -= change;
			next.easeInWeight += change;
		}
		
		public static float GetWeight(this ICustomKeyframe current, ICustomKeyframe next)
		{
			EnsureWeightSumIs1(current, next);
			var sum = current.easeOutWeight + next.easeInWeight;
			if (sum > 0)
				return Mathf.Lerp(0, 1, next.easeInWeight / sum);
			return .5f;
		}

		private static void EnsureWeightSumIs1(this ICustomKeyframe current, ICustomKeyframe next)
		{
			var sum = current.easeOutWeight + next.easeInWeight;
			if (Math.Abs(sum - 1) > .01f)
			{
				if (sum == 0)
				{
					current.easeOutWeight = .5f;
					next.easeInWeight = .5f;
				}
				else
				{
					current.easeOutWeight = Mathf.Lerp(0, 1, current.easeOutWeight / sum);
					next.easeInWeight = 1 - current.easeOutWeight;
				}
			}
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

		private static ProfilerMarker _enumerateTypeFieldsMarker = new ProfilerMarker("EnumerateTypeFields");
		private static ProfilerMarker _findFieldsMarker = new ProfilerMarker("CacheTypeFields");

		private class CachedType
		{
			public Type Type;
			public BindingFlags Flags;
			public FieldInfo[] Fields;
		}

		private static readonly List<CachedType> cachedFields = new List<CachedType>();

		internal static IEnumerable<FieldInfo> EnumerateFields(this Type type,
			Predicate<FieldInfo>? take = null,
			BindingFlags flags = BindingFlags.Default | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)

		{
			using (_enumerateTypeFieldsMarker.Auto())
			{
				var entry = cachedFields.FirstOrDefault(c => c.Type == type && c.Flags == flags);
				if (entry == null)
				{
					using (_findFieldsMarker.Auto())
					{
						entry = new CachedType() { Type = type, Flags = flags, Fields = Array.Empty<FieldInfo>()};
						cachedFields.Add(entry);

						if (typeof(ICollection).IsAssignableFrom(type))
						{ 
							if (type.IsGenericType)
							{
								var content = type.GetGenericArguments().FirstOrDefault();
								if (content != null)
								{
									entry.Fields = content.GetFields(flags);
								}
								else throw new Exception("Failed getting collection content type");
							}
							else throw new Exception("Failed getting collection content type");
						}
						else
						{
							entry.Fields = type.GetFields(flags);
						}
					}
				}

				foreach (var field in entry.Fields)
				{
					if (take != null && !take.Invoke(field))
						continue;
					yield return field;
				}
			}
		}
	}
}
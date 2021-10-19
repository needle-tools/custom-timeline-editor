using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Needle.Timeline
{
	public static class ComputeStructUtils
	{
		private static readonly List<(Type type, int? stride)> lookup = new List<(Type type, int? stride)>();
		private static readonly BindingFlags flags = BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public;

		public static int GetStride<T>(this T _) where T : struct
		{
			return GetStride<T>();
		}

		public static int GetStride<T>() where T : struct
		{
			var t = typeof(T);
			return t.GetStride();
		}

		public static int GetStride(this Type type)
		{
			return InternalGetStride(type, 0);
		}

		private static int InternalGetStride(this Type type, int level)
		{
			var existing = lookup.FirstOrDefault(l => l.type == type);
			if (existing.stride.HasValue) return existing.stride.Value;
			if (level > 1)
			{
				lookup.Add((type, 0));
				throw new Exception("Level exceeded: " + level);
			}
			var sum = 0;
			if (typeof(IList).IsAssignableFrom(type) && type.IsGenericType)
			{ 
				var gt = type.GetGenericArguments().FirstOrDefault();
				if (gt == null) throw new Exception("Failed getting generic");
				if (gt.IsPrimitive)
					sum += GetSize(gt, level);
				else 
					sum += InternalGetStride(gt, level);
			}
			if (sum == 0)
			{
				foreach (var m in type.GetFields(flags)) 
					sum += GetSize(m.FieldType, level);
			}
			lookup.Add((type, sum));
			return sum;
		}

		private static int GetSize(Type type, int level)
		{
			if (type == typeof(float))
				return sizeof(float);
			if (type == typeof(double))
				return sizeof(double);
			
			if (type == typeof(int))
				return sizeof(int);
			if (type == typeof(long))
				return sizeof(long);
			
			if (type == typeof(uint))
				return sizeof(uint);
			if (type == typeof(byte))
				return sizeof(byte);

			if (type.IsValueType)
			{
				return InternalGetStride(type, ++level);
				// Debug.LogError("Unhandled type " + type);
			}
			return 0;
		}
	}
}
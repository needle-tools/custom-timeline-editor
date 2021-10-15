using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Unity.Profiling;
using UnityEngine;

namespace Needle.Timeline
{
	[Priority(-100), NoAutoSelect]
	public class ReflectiveInterpolatable : IInterpolatable
	{
		public static bool TryCreate(Type type, out ReflectiveInterpolatable ri)
		{
			var fields = type.GetFields(flags);
			ri = new ReflectiveInterpolatable();
			for (var index = 0; index < fields.Length; index++)
			{
				var field = type.GetFields(flags)[index];
				if (!InterpolatorBuilder.TryFindInterpolatable(field.FieldType, out var interpolatable, false))
				{
					Debug.LogWarning("No interpolatable found for " + field.FieldType);
					return false;
				}
				ri.data.Add(new MemberInterpolationData(field, field.FieldType, interpolatable));
			}
			return ri.data.Count > 0;
		}
		
		private const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		private readonly List<MemberInterpolationData> data = new List<MemberInterpolationData>();
		 
		

		public void Interpolate(ref object instance, object obj0, object obj1, float t)
		{
			foreach (var entry in data)
			{
				entry.Interpolate(ref instance, obj0, obj1, t);
			}
		}

		[NoAutoSelect]
		private class MemberInterpolationData : IInterpolatable
		{
			private readonly MemberInfo member;
			private readonly IInterpolatable interpolatable;
			private object _valueInstance;

			private static ProfilerMarker marker = new ProfilerMarker(nameof(MemberInterpolationData) + ".Interpolate");

			public MemberInterpolationData(MemberInfo member, Type valueType, IInterpolatable interpolatable)
			{
				this.member = member;
				this.interpolatable = interpolatable;
				_valueInstance = Activator.CreateInstance(valueType);
			}

			public void Interpolate(ref object instance, object obj0, object obj1, float t)
			{
				using var _ = marker.Auto();
				
				if (obj0 == null && obj1 == null)
				{
					instance = null;
					return; 
				} 

				// TODO: optimize this
				// this produces a lot of garbage right now
				// https://stackoverflow.com/questions/16073091/is-there-a-way-to-create-a-delegate-to-get-and-set-values-for-a-fieldinfo
				
				var c0 = obj0 != null ? member.Get(obj0) : null;
				var c1 = obj1 != null ? member.Get(obj1) : null;
				if(c0 != null && c1 != null)
					interpolatable.Interpolate(ref _valueInstance, c0, c1, t);
				member.Set(instance, _valueInstance);
			}
		}
	}
	
	internal static class FieldInfoExtensions
	{
		public static Func<S, T> CreateGetFieldDelegate<S, T>(this FieldInfo fieldInfo)
		{
			var instExp = Expression.Parameter(typeof(S));
			var fieldExp = Expression.Field(instExp, fieldInfo);
			return Expression.Lambda<Func<S, T>>(fieldExp, instExp).Compile();
		}
	}
}
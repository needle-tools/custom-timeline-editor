using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using UnityEditor;

#nullable enable

namespace Needle.Timeline
{
	public struct FieldValueInfo
	{
		public Type ValueType;
		public FieldInfo Field;
	}
	
	public class ShaderValueConversionHandler : TypesSupportAttribute
	{
		public ShaderValueConversionHandler(Type supportedType)
		{
			SupportedType = supportedType;
		}

		public ShaderValueConversionHandler(params Type[] supportedTypes)
		{
			this.SupportedTypes = supportedTypes;
		}
	}

	public interface IShaderValueConversionHandler
	{
		object Convert(object value, FieldInfo field);
		void Convert(object value, FieldInfo field, IList list, int index);
	}
	
	public interface IShaderValueConversionHandler<out TResult> : IShaderValueConversionHandler where TResult : struct
	{
		new TResult Convert(object value, FieldInfo field);
	}

	public static class CustomCollectionTypeBuilder
	{
		// this could allow to automatically set type e.g. transform to float3 or transform to struct{float3 pos, float scale}

		private class Implementation
		{
			public Type Type;
			public ShaderValueConversionHandler? Attribute;

			public Implementation(Type type)
			{
				Type = type;
				Attribute = type.GetCustomAttribute<ShaderValueConversionHandler>();
			}
		}

		private static Implementation[]? cache;
		
		public static IShaderValueConversionHandler? Build(Type objectType)
		{
			if (cache == null)
			{
				var types = RuntimeTypeCache.GetTypesDerivedFrom<IShaderValueConversionHandler>().ToArray();
				cache = new Implementation[types.Length];
				var index = 0;
				foreach (var t in types)
				{
					var impl = new Implementation(t);
					cache[index++] = impl;
				}
				cache = cache.OrderByDescending(i => i.Attribute?.Priority ?? 0).ToArray();
			}

			foreach (var impl in cache)
			{
				if (impl.Attribute?.Supports(objectType) ?? false)
				{
					return impl.Type.TryCreateInstance() as IShaderValueConversionHandler;
				}
			}
			
			return null;
		}
	}
}
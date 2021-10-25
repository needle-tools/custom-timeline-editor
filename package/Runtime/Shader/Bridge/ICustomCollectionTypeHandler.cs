using System;
using System.Collections;
using System.Reflection;

namespace Needle.Timeline
{
	public class ShaderValueConversionHandler : Attribute
	{
		
	}

	public interface IShaderValueConversionHandler
	{
		object Convert(object value, FieldInfo field);
		void Convert(object value, FieldInfo field, IList list, int index);
	}
	
	public interface IShaderValueConversionHandler<out TResult> where TResult : struct
	{
		TResult Convert(object value, FieldInfo field);
	}

	public static class CustomCollectionTypeBuilder
	{
		// this could allow to automatically set type e.g. transform to float3 or transform to struct{float3 pos, float scale}
		
		public static IShaderValueConversionHandler Build(Type valueType, FieldInfo field)
		{
			// TypeCache.GetTypesDerivedFrom<IShaderValueConversionHandler>()
			return null;
		}
	}
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

#nullable enable

namespace Needle.Timeline
{
	public static class ShaderBridgeBuilder
	{
		private class ImplementationInfo
		{
			public readonly Type Type;
			public readonly ShaderBridgeAttribute? Attribute;

			public ImplementationInfo(Type type, ShaderBridgeAttribute? attribute)
			{
				Type = type;
				Attribute = attribute;
			}

			public bool Supports(Type type) => Attribute?.Supports(type) ?? false;
		}
		
		private static List<ImplementationInfo>? shaderBridgeTypes;
		
		public static IShaderBridge? BuildMapping(FieldInfo field)
		{
			if (shaderBridgeTypes == null)
			{
				shaderBridgeTypes = new List<ImplementationInfo>();
				var types = RuntimeTypeCache.GetTypesDerivedFrom<IShaderBridge>();
				foreach (var type in types)
				{
					var att = type.GetCustomAttribute<ShaderBridgeAttribute>();
					shaderBridgeTypes.Add(new ImplementationInfo(type, att));
				}
				shaderBridgeTypes.Sort((x,y) =>  (y.Attribute?.Priority??0)-(x.Attribute?.Priority??0));
			}

			var match = shaderBridgeTypes.FirstOrDefault(e => e.Supports(field.FieldType));
			if (match != null)
				return Activator.CreateInstance(match.Type) as IShaderBridge;
			return null;
		}
	}
}
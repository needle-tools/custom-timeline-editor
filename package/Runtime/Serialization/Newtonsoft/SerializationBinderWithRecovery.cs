#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace Needle.Timeline.Serialization
{
	public class SerializationBinderWithRecovery : ISerializationBinder
	{
		private readonly ISerializationBinder forward;
		// private List<(Type type, RefactorInfo info)>? typesWithRefactorInfo;

		public SerializationBinderWithRecovery(ISerializationBinder? fallback = null)
		{
			this.forward = fallback ?? new DefaultSerializationBinder();
		}
		
		public Type BindToType(string? assemblyName, string typeName)
		{
			try
			{
				return forward.BindToType(assemblyName, typeName);
			}
			catch (JsonSerializationException)
			{
				var res = ResolveType(assemblyName, typeName);
				if (res != null) return res;
				throw;
			}
		}

		public void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
		{
			forward.BindToName(serializedType, out assemblyName, out typeName);
		}

		private static Type? ResolveType(string? assemblyName, string typeName)
		{
			// Debug.Log("Try resolve " + assemblyName + ", " + typeName);
			foreach (var type in RuntimeTypeCache.Types)
			{
				var info = type.GetCustomAttribute<RefactorInfo>();
				if (info == null) continue;
				var typeAssemblyName = type.Assembly.GetName().Name;  
				if (!string.IsNullOrEmpty(info.OldName) && typeName.EndsWith(info.OldName!) && typeAssemblyName == assemblyName)
				{ 
					// Debug.Log("FOUND " + typeName + ", is now: " + type.FullName);
					return type; 
				} 
				if(!string.IsNullOrEmpty(info.OldAssemblyName) && info.OldAssemblyName == assemblyName)
				{
					if (typeName.EndsWith(type.Name)) 
					{
						return type;
					}
					
					if (!string.IsNullOrEmpty(info.OldName) && typeName.EndsWith(info.OldName!))
					{
						return type;
					}
				}
			}
			return null;
		}
	}
}
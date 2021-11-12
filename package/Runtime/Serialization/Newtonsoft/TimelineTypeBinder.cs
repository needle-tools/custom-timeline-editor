#nullable enable
using System;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace Needle.Timeline.Serialization
{
	public class TimelineSerializationBinder : ISerializationBinder
	{
		private readonly ISerializationBinder forward;

		public TimelineSerializationBinder(ISerializationBinder? fallback = null)
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

		private Type? ResolveType(string? assemblyName, string typeName)
		{
			Debug.Log("Try resolve " + assemblyName + ", " + typeName);
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assembly in assemblies)
			{
				if (assembly.GetName().Name == assemblyName)
				{
					foreach (var type in assembly.GetTypes())
					{
						var info = type.GetCustomAttribute<RefactorInfo>();
						if (info == null) continue;
						if (string.IsNullOrEmpty(info.OldName)) continue;
						var oldTypeName = info.OldName;
						if (typeName.EndsWith(oldTypeName))
						{
							Debug.Log("FOUND " + typeName + ", is now: " + type.FullName);
							return type;
						}
					}
				}
			}
			return null;
		}
	}
}
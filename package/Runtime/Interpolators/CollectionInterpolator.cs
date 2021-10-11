using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Needle.Timeline.Interfaces;
using UnityEngine;

namespace Needle.Timeline
{
	[Priority(-100)]
	// ReSharper disable once UnusedType.Global
	public class CollectionInterpolator<T> : IInterpolator<T>
	{
		public object Instance { get; set; }

		public bool CanInterpolate(Type type)
		{
			if (typeof(IList).IsAssignableFrom(type)) return true;
			// var genericList = typeof(IList<>).MakeGenericType(type);
			// if (genericList.IsAssignableFrom(type)) return true;
			// var typeArgs = type.GenericTypeArguments;
			// if (typeArgs.Length == 1)
			// {
			// 	var genericParam = typeArgs[0];
			// 	
			// }
			return false;
		}

		private IList buffer;
		private IList output;
		private Type listContentType;

		public T Interpolate(T v0, T v1, float t)
		{
			Debug.Log(v0);
			return v1;
		}

		public object Interpolate(object v0, object v1, float t)
		{
			if (v0 == null && v1 == null) return null;

			var list0 = v0 as IList;
			var list1 = v1 as IList;

			buffer ??= (IList)Activator.CreateInstance(v0?.GetType() ?? v1.GetType());
			buffer.Clear();

			if (listContentType == null)
			{
				listContentType = (list0 ?? list1)?.GetType().GenericTypeArguments.FirstOrDefault();
				if (listContentType != null) Debug.Log(listContentType);
			}


			var count = Mathf.RoundToInt(Mathf.Lerp(list0?.Count ?? 0, list1?.Count ?? 0, t));
			for (var i = 0; i < count; i++)
			{
				var val0 = list0?.Count > 0 ? list0[i % list0.Count] : list1?[i];
				var val1 = list1?.Count > 0 ? list1[i % list1.Count] : list0?[i];
				if (val0 == null && val1 == null)
				{
					buffer.Add(null);
					continue;
				}
				
				if (val0 is IInterpolatable i0 && val1 is IInterpolatable i1)
				{
					object instance;
					if (output?.Count > i)
						instance = output[i];
					else
					{
						var type = val0?.GetType() ?? val1.GetType();
						instance = Activator.CreateInstance(type);
					}
					var interpolatableInstance = (IInterpolatable)instance;
					i0.Interpolate(ref interpolatableInstance, i0, i1, t);
					buffer.Add(interpolatableInstance);
				}
				// TODO: remove once we have support for interpolation helpers
				else if (val0 is Vector3 vec0 && val1 is Vector3 vec1)
				{
					var res = Vector3.Lerp(vec0, vec1, t);
					buffer.Add(res);
				}
				else Debug.LogError("Can not interpolate " + listContentType);
			}
			output ??= (IList)Activator.CreateInstance(v0?.GetType() ?? v1.GetType());
			output.Clear();
			foreach (var obj in buffer)
				output.Add(obj);
			return output;
		}
	}
}
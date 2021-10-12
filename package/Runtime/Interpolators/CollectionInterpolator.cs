using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Needle.Timeline
{
	public enum CollectionInterpolationMode
	{
		AllAtOnce = 0,
		Individual = 1
	}
	
	[Priority(-100)]
	// ReSharper disable once UnusedType.Global
	public class CollectionInterpolator : IInterpolator
	{
		public CollectionInterpolationMode CurrentMode = CollectionInterpolationMode.Individual;
		
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
		private IInterpolatable interpolatable;
		private bool didSearchInterpolatable;

		public object Interpolate(object v0, object v1, float t)
		{
			if (v0 == null && v1 == null) return null;
			
			var list0 = v0 as IList;
			var list1 = v1 as IList;
			if (list0?.Count <= 0 && list1?.Count <= 0) return null;
			var listType = v0?.GetType() ?? v1.GetType();
			TryFindListContentType(list0);
			TryFindListContentType(list1);
			if (listContentType == null) return null;
			if (!didSearchInterpolatable)
			{
				didSearchInterpolatable = true;
				Interpolators.TryFindInterpolatable(listContentType, out interpolatable);
			}

			buffer ??= (IList)Activator.CreateInstance(listType);
			buffer.Clear();

			var count = Mathf.RoundToInt(Mathf.Lerp(list0?.Count ?? 0, list1?.Count ?? 0, t));
			var perEntry = 1f / count;
			
			for (var i = 0; i < count; i++)
			{
				var val0 = list0?.Count > 0 ? list0[i % list0.Count] : list1?[i];
				var val1 = list1?.Count > 0 ? list1[i % list1.Count] : list0?[i];
				if (val0 == null && val1 == null)
				{
					buffer.Add(null);
					continue;
				}
				var pos = t;
				switch (CurrentMode)
				{
					case CollectionInterpolationMode.Individual:
						var start = i * perEntry;
						pos = Mathf.Clamp01(t - start) / perEntry;
						break;
				}
				if (interpolatable == null)
				{
					if (pos < 1) buffer.Add(val0);
					else buffer.Add(val1);
					continue;
				}
				object instance;
				if (output?.Count > i)
					instance = output[i];
				else
				{
					var type = val0?.GetType() ?? val1.GetType();
					instance = Activator.CreateInstance(type);
				}
				// Debug.Log(t + ": " + pos.ToString("0.000") + ", " + count + ", " + i);
				interpolatable.Interpolate(ref instance, val0, val1, pos);
				buffer.Add(instance);
			}
			output ??= (IList)Activator.CreateInstance(v0?.GetType() ?? v1.GetType());
			output.Clear();
			foreach (var obj in buffer)
				output.Add(obj);
			return output;
		}

		private void TryFindListContentType(IEnumerable list)
		{
			if (listContentType != null) return;
			if (list == null) return;
			listContentType = list.GetType().GenericTypeArguments.FirstOrDefault();
			if (listContentType != null) return;
			foreach (var e in list)
			{
				if (listContentType != null) break;
				listContentType = e?.GetType();
			}
		}
	}
}
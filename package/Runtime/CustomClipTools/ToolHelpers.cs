using System.Collections;
using System.Reflection;
using UnityEngine;

namespace Needle.Timeline
{
	public static class ToolHelpers
	{
		public static Vector3? TryGetPosition(object valueOwner, object value)
		{
			// if (keyframe.value == null) return null;
			// var val = keyframe.value;
			// if (val is IList list)
			// {
			// 	var obj = 
			// }

			if (valueOwner == value && value is Vector3 pos)
			{
				return pos;
			}
			if (valueOwner != null)
			{
				foreach (var field in valueOwner.GetType().GetRuntimeFields())
				{
					if (field.FieldType == typeof(Vector3))
					{
						return (Vector3)field.GetValue(valueOwner);
					}
				}
			}
			return null;
		}
	}
}
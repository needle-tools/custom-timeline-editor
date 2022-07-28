using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Needle.Timeline
{
	public static class ToolHelpers
	{
		public static Vector3? TryGetPosition(object valueOwner, object value)
		{
			if (value != null && value is Vector3)
			{
				return (Vector3)value;
			}
			if (valueOwner != null)
			{
				foreach (var field in valueOwner.GetType().GetRuntimeFields())
				{
					if (field.FieldType == typeof(Vector3))
					{
						return (Vector3)field.GetValue(valueOwner);
					}
					if (field.FieldType == typeof(Vector2))
					{
						return (Vector3)field.GetValue(valueOwner).Cast(typeof(Vector3));
					}
				}
			}
			return null;
		}
	}
}
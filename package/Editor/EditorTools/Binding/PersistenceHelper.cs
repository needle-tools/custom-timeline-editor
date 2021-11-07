using System.Reflection;
using UnityEngine;

namespace Needle.Timeline
{
	internal static class PersistenceHelper
	{
		public static bool TryGetPreviousValue(FieldInfo field, out object value)
		{
			// TODO: here we should handle restore previously set values 
			value = null;
			return false;
		}

		public static void OnValueChanged(FieldInfo field, object newValue)
		{
			// Debug.Log(field.Name + " changed: " + newValue); 
		}
	}
}
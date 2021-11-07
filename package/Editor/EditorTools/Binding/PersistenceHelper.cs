using System.Reflection;

namespace Needle.Timeline
{
	internal static class PersistenceHelper
	{
		public static bool TryGetPreviousValue( FieldInfo field, out object value)
		{
			value = null;
			return false;
		}
	}
}
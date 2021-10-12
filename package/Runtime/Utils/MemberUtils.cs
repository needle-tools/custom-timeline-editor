using System.Reflection;

namespace Needle.Timeline
{
	public static class MemberUtils
	{
		public static void Set(this MemberInfo member, object target, object value)
		{
			switch (member)
			{
				case FieldInfo field:
					field.SetValue(target, value);
					break;
				case PropertyInfo property:
					if (property.CanWrite)
						property.SetValue(target, value);
					break;
			}
		}

		public static object Get(this MemberInfo member, object target)
		{
			switch (member)
			{
				case FieldInfo field:
					return field.GetValue(target);
				
				case PropertyInfo property:
					if (property.CanRead)
						return property.GetValue(target);
					break;
			}
			
			return null;
		}
	}
}
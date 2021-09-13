using System;
using System.Reflection;
using Editor;
using UnityEditor.Android;

namespace Needle.Timeline
{
	public readonly struct MemberWrapper : IValueHandler
	{
		private readonly MemberInfo member;
		private readonly Func<object> getTarget;
		private readonly Type targetType;

		public MemberWrapper(MemberInfo member, Func<object> getTarget, Type targetType)
		{
			this.member = member;
			this.getTarget = getTarget;
			this.targetType = targetType;
		}

		public void SetValue(object value)
		{
			var target = getTarget();
			if (target == null)
			{
				// Debug.Log("Target is null, not setting " + value);
				return;
			}

			if (value.GetType() != targetType)
			{
				value = Convert.ChangeType(value, targetType);
			}

			// Debug.Log("set " + value + " on " + target, target as UnityEngine.Object);
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
	}
}
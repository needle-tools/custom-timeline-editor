using System;
using System.Reflection;
using System.Text;
using Editor;
using UnityEditor.Android;
using UnityEngine;

namespace Needle.Timeline
{
	public readonly struct MemberWrapper : IValueHandler
	{
		private readonly MemberInfo member;
		private readonly Func<object> getTarget;
		private readonly Type targetType;
		private readonly bool setSafe;

		public MemberWrapper(MemberInfo member, Func<object> getTarget, Type targetType)
		{
			this.member = member;
			this.getTarget = getTarget;
			this.targetType = targetType;
			this.setSafe = true;
		}

		public void SetValue(object value)
		{
			var target = getTarget();
			Debug.Log("Set " + member.Name + " on " + target?.GetType());
			if (target == null)
			{
				// Debug.Log("Target is null, not setting " + value);
				return;
			}

			if (value?.GetType() != targetType)
			{
				value = Convert.ChangeType(value, targetType);
			}

			if (setSafe)
			{
				try
				{
					SetDirect(target, value);
				}
				catch (Exception e)
				{
					Debug.LogError("Failed setting value for <b>" + member.Name + "</b> on <i>" + target?.GetType()?.Name + "</i>");
					Debug.LogException(e);
				}
			}
			else
			{
				SetDirect(target, value);
			}
		}

		private void SetDirect(object target, object value)
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
	}
}
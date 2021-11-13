using System;
using System.Collections.Generic;

namespace Needle.Timeline.CurveEasing
{
	public class EasingRegistry : ImplementorsRegistry<ICurveEasing>
	{
		private static EasingRegistry instance;
		public static EasingRegistry Instance => instance ??= new EasingRegistry();

	}

	public static class EasingExtensions
	{
		private static readonly List<IArgument> _creationArgs = new List<IArgument>();

		public static bool SetEasing(this ICustomClip clip, Type easingType)
		{
			if (!typeof(ICurveEasing).IsAssignableFrom(easingType)) return false;
			if (clip is IHasEasing e)
			{
				if (e.DefaultEasing != null && e.DefaultEasing.GetType() == easingType)
				{
					return true;
				}
				_creationArgs.Clear();
				_creationArgs.Add(clip.AsArg());
				
				if (EasingRegistry.Instance.TryGetNewInstance(easingType, out var obj, _creationArgs))
				{
					e.DefaultEasing = (ICurveEasing)obj;
					return true;
				}
			}
			return false;
		}

		public static bool IsCurrentDefaultEasingType(this ICustomClip clip, Type easingType)
		{
			if (!typeof(ICurveEasing).IsAssignableFrom(easingType)) return false;
			if (clip is IHasEasing e && e.DefaultEasing != null)
			{
				return e.DefaultEasing.GetType() == easingType;
			}
			return false;
		}

		public static bool TryGetEasing(this ICustomClip clip, out ICurveEasing easing)
		{
			if (clip is IHasEasing e && e.DefaultEasing != null)
			{
				easing = e.DefaultEasing;
				return true;
			}
			easing = null;
			return false;
		}
	}
}
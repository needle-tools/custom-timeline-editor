using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace Needle.Timeline
{
	public static class IAnimatedExtensions
	{
		public static bool IsIn2DMode(this IAnimated _)
		{
#if UNITY_EDITOR
			return SceneView.lastActiveSceneView?.in2DMode ?? false;
#else
			return false;
#endif
		}

		public static bool IsVisible(this IAnimated _)
		{
			return !IsHidden(_);
		}

		public static bool IsHidden(this IAnimated _)
		{
#if UNITY_EDITOR
			if (_ is Component obj)
				return IsHidden(obj);
#endif
			return false;
		}

		public static bool IsHidden(this Component _)
		{
#if UNITY_EDITOR
			return SceneVisibilityManager.instance.IsHidden(_.gameObject);
#else
			return false;
#endif
		}
	}
}
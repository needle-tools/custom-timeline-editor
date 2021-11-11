using System.Runtime.InteropServices;
using System.Threading.Tasks;
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

		public static Task RequestBuffer(this IAnimated _, float duration = 10)
		{
#if UNITY_EDITOR
			return TimelineBuffer.RequestBufferCurrentInspectedTimeline(duration);
#else
			return Task.CompletedTask;
#endif
		}

		private static float time;
		private static float deltaTime;
		internal static float? deltaTimeOverride = null;

		internal static void OnProcessFrame(this IAnimated _, FrameInfo info)
		{
			time = info.Time;
			deltaTime = deltaTimeOverride ?? info.DeltaTime;
		}

		public static float DeltaTime(this IAnimated _) => deltaTime;

		public static void SetTime(this IAnimated _, ComputeShader shader)
		{
			var vec = new Vector4(time, Mathf.Sin(time), Mathf.Cos(time), deltaTime);
			// Debug.Log(vec.x + " = " + vec.w.ToString("0.000000"));
			shader.SetVector("_Time", vec);
		}
	}
}
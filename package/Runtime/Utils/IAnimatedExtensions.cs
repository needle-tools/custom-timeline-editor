using UnityEditor;

namespace Needle.Timeline
{
	public static class IAnimatedExtensions
	{
		public static bool IsIn2DMode(this IAnimated _) => SceneView.lastActiveSceneView?.in2DMode ?? false;
	}
}
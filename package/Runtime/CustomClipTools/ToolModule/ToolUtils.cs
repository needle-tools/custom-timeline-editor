using UnityEngine;

namespace Needle.Timeline
{
	public static class ToolUtils
	{
		public static float? GetRadiusDistanceScreenSpace(this InputData input, float radius, Vector3 worldPoint)
		{
			var wp = input.WorldPosition.GetValueOrDefault();
			var sp = input.ScreenPosition;
			var screenRadius = input.ToScreenRadius(wp, sp, radius);
			var point = input.ToScreenPoint(worldPoint);
			return Vector2.Distance(point, sp) / screenRadius;
		}
	}
}
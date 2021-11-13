using UnityEngine;

namespace Needle.Timeline
{
	public static class ToolUtils
	{
		public static float? GetRadiusDistanceScreenSpace(this InputData input, float radius, Vector3 worldPoint)
		{
			var point = input.ToScreenPoint(worldPoint);
			return GetRadiusDistanceScreenSpace(input, radius, point);
		}
		
		public static float? GetRadiusDistanceScreenSpace(this InputData input, float radius, Vector2 screenPoint)
		{
			var wp = input.WorldPosition.GetValueOrDefault();
			var sp = input.ScreenPosition;
			var screenRadius = ToScreenRadius(wp, sp, radius);
			var point = screenPoint;
			return Vector2.Distance(point, sp) / screenRadius;
		}

		public static float? GetRadiusInPixel(this InputData input, float radius)
		{
			if(input.WorldPosition != null)
				return ToScreenRadius(input.WorldPosition.Value, input.ScreenPosition, radius);
			return null;
		}

		public static float? GetLineDistanceScreenSpace(this InputData input, float radius, Vector3 worldPointFrom, Vector3 worldPointTo)
		{
			var wp = input.WorldPosition.GetValueOrDefault();
			var sp0 = input.ToScreenPoint(worldPointFrom);
			var sp1 = input.ToScreenPoint(worldPointTo);
			var sp = input.ToScreenPoint(wp);
#if UNITY_EDITOR
			var dist = UnityEditor.HandleUtility.DistancePointLine(sp, sp0, sp1);
			var screenRadius = ToScreenRadius(wp, input.ScreenPosition, radius);
			
			return dist / screenRadius;
#else
			throw new System.NotImplementedException("Distance Point to Line is not implemented for runtime yet");
#endif
		}
		
		private static float ToScreenRadius(Vector3 worldPoint, Vector2 screenPoint, float radius)
		{
			var cam = Camera.current;
			var t = cam.transform;
			var offset = t.right * radius;
			var wp2 = worldPoint + offset;
			var pt1 = screenPoint;
			var pt2 = Camera.current.WorldToScreenPoint(wp2);
			return Mathf.Abs(pt1.x - pt2.x);
		}
	}
}
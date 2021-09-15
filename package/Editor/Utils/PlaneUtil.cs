using UnityEngine;

namespace Needle.Timeline
{
	public static class PlaneUtils
	{
		public static Vector3? GetPointOnPlane(Camera cam, out Ray ray, out Plane plane, out float dist)
		{
#if UNITY_EDITOR
			var mp = Application.isPlaying ? (Vector2)Input.mousePosition : Event.current.mousePosition;
			if (!Application.isPlaying)
			{
				mp.y = Screen.height - mp.y;
				mp.y -= 40;
			}
#else
			var mp = Input.mousePosition;
#endif

			ray = cam.ScreenPointToRay(mp);
			return GetPointOnPlane(ray, out plane, out dist);
		}

		public static Vector3? GetPointOnPlane(Ray ray, out Plane plane, out float distance)
		{
			return GetPointOnPlane(ray.direction, ray.origin, out plane, out distance);
		}

		public static Vector3? GetPointOnPlane(Vector3 viewDirection, Vector3 origin, out Plane plane, out float distance)
		{
			plane = GetPlane(viewDirection);
			if (plane.Raycast(new Ray(origin, viewDirection), out var center))
			{
				distance = center;
				return origin + viewDirection * center;
			}

			distance = 0;
			return null;
		}

		public static Plane GetPlane(Vector3 viewDirection)
		{
			var floor = Vector3.Dot(viewDirection, Vector3.up);
			var wallRight = Vector3.Dot(viewDirection, Vector3.right);
			var wallBack = Vector3.Dot(viewDirection, Vector3.forward);
			// Debug.Log(floor.ToString("0.0") + ", " + wallRight.ToString("0.0") + ", " + wallBack.ToString("0.0"));
			var floor_abs = Mathf.Abs(floor);
			var right_abs = Mathf.Abs(wallRight);
			var back_abs = Mathf.Abs(wallBack);
			if (floor_abs > right_abs && floor_abs > back_abs)
			{
				// Debug.Log("up");
				var plane = new Plane(floor_abs > 0 ? Vector3.up : Vector3.down, Vector3.zero);
				return plane;
			}

			if (right_abs > back_abs)
			{
				// Debug.Log("right");
				var plane = new Plane(right_abs > 0 ? Vector3.right : Vector3.left, Vector3.zero);
				return plane;
			}
			else
			{
				// Debug.Log("back");
				var plane = new Plane(back_abs > 0 ? Vector3.forward : Vector3.back, Vector3.zero);
				return plane;
			}
		}
	}
}
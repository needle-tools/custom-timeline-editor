using UnityEngine;

namespace Needle.Timeline
{
	public static class PlaneUtils
	{
		public static Vector3 GetPointInWorld(Camera cam, out Vector3 normal)
		{
			var mp = GetMousePoint();
			var ray = cam.ScreenPointToRay(mp);
			var rc = Physics.Raycast(ray, out var rch);
			if (rc)
			{
				normal = rch.normal;
				return rch.point;
			}
			var point = GetPointOnPlane(cam, out _, out var plane, out _);
			normal = plane.normal;
			return point;
		}

		public static Vector2 GetMousePoint()
		{
#if UNITY_EDITOR
			var mp = Event.current.mousePosition;
			mp.y = Screen.height - mp.y;
			mp.y -= 40;
#else
			var mp = Input.mousePosition;
#endif
			return mp;
		}

		public static Vector3 GetPointOnPlane(Camera cam, out Ray ray, out Plane plane, out float dist)
		{
			var mp = GetMousePoint();
			ray = cam.ScreenPointToRay(mp);
			return GetPointOnPlane(ray, out plane, out dist, cam.transform.forward);
		}

		public static Vector3 GetPointOnPlane(Ray ray, out Plane plane, out float distance, Vector3? viewDirection = null)
		{
			plane = GetPlane(viewDirection ?? ray.direction);
			if (plane.Raycast(ray, out var center))
			{
				distance = center;
				return ray.origin + ray.direction * center;
			}

			distance = 0;
			return Vector3.zero;
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

		public static Vector3 GetPointOnCameraPlane(Camera cam, Vector3 cameraPivot, out float distance)
		{
			var mp = GetMousePoint();
			var ray = cam.ScreenPointToRay(mp);
			var plane = new Plane(-cam.transform.forward, cameraPivot);
			if (plane.Raycast(ray, out var center))
			{
				distance = center;
				return ray.origin + ray.direction * center;
			}

			distance = 0;
			return Vector3.zero;
		}
	}
}
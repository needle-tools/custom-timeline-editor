using UnityEngine;

namespace Needle.Timeline
{
	public class ToolDataProxy : IToolData
	{
		// actual data sources
		internal InputData input;
		
		// the radius is per tool module
		internal float? _radius;

		float? IToolData.Radius
		{
			get => _radius;
			set => _radius = value;
		}

		public Vector3? StartWorldPosition => input.StartWorldPosition;

		public Vector3? WorldPosition => input.WorldPosition;

		public Vector3? DeltaWorld => input.DeltaWorld;

		public Vector2 ScreenPosition => input.ScreenPosition;

		public Vector2 ToScreenPoint(Vector3 worldPoint)
		{
			return input.ToScreenPoint(worldPoint);
		}

		public IToolEventContext? Context
		{
			get => input.Context;
			set => input.Context = value;
		}
	}
}
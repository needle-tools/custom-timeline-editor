using UnityEditor;
using UnityEngine;

namespace Needle.Timeline
{
	public class InputData : IToolData
	{
		public bool IsIn2DMode;
		
		
		public Vector3? WorldPosition { get; private set; }
		public Vector3? WorldNormal;
		public Vector3? LastWorldPosition;
		public Vector3? StartWorldPosition { get; private set; }

		private Vector3? deltaWorld;

		public Vector3? DeltaWorld
		{
			get
			{
				if (deltaWorld == null && WorldPosition.HasValue && LastWorldPosition.HasValue)
					return deltaWorld = WorldPosition.Value - LastWorldPosition.Value;
				return deltaWorld.GetValueOrDefault();
			}
			private set => deltaWorld = value;
		}

		public Vector2 ToScreenPoint(Vector3 worldPoint)
		{
			return Camera.current.WorldToScreenPoint(worldPoint);
		}

		public Vector2 ScreenPosition;
		public Vector2 LastScreenPosition;
		public Vector2 StartScreenPosition;
		
		public Vector2 ScreenDelta => ScreenPosition - LastScreenPosition;
		public InputEventStage Stage;

		internal void Update()
		{
			var evt = Event.current;
			if (evt == null) return;
			if (evt.type == EventType.Used) return;

#if UNITY_EDITOR
			IsIn2DMode = SceneView.lastActiveSceneView?.in2DMode ?? false;
#endif
			
			switch (evt.type)
			{
				case EventType.MouseDown:
					Stage = InputEventStage.Begin;
					break;
				case EventType.MouseDrag:
					Stage = InputEventStage.Update;
					break;
				case EventType.MouseUp:
					Stage = InputEventStage.End;
					break;
				default:
					Stage = InputEventStage.Unknown;
					break;
			}

			void RecordCurrent()
			{
				DeltaWorld = null;
				LastWorldPosition = WorldPosition;
				WorldPosition = PlaneUtils.GetPointInWorld(Camera.current, out var normal);
				WorldNormal = normal;
				LastScreenPosition = ScreenPosition;
				ScreenPosition = evt.mousePosition;
				ScreenPosition.y = Screen.height - ScreenPosition.y;
			}
			
			switch (evt.type)
			{
				case EventType.MouseDown:
					RecordCurrent();
					StartWorldPosition = WorldPosition;
					StartScreenPosition = ScreenPosition;
					break;
				case EventType.MouseDrag:
				case EventType.MouseUp:
				case EventType.MouseMove:
					RecordCurrent();
					break;
			}
		}
	}
}
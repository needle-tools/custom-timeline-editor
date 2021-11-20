using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	public class InputData
	{
		public bool IsIn2DMode;
		
		public Vector3? WorldPosition { get; private set; }
		public Vector3? WorldNormal;
		public Quaternion? ViewRotation { get; private set; }
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

		public IToolEventContext? Context { get; set; }

		public Ray ToRay(Vector2 screenPoint)
		{
			return Camera.current.ScreenPointToRay(screenPoint);
		}

		public Vector2 ScreenPosition { get; private set; }
		public Vector2 LastScreenPosition;
		public Vector2 StartScreenPosition;
		
		public Vector2 ScreenDelta => ScreenPosition - LastScreenPosition;
		public InputEventStage Stage;

		public MouseButton Button;
		public EventModifiers Modifiers;
		
		private KeyCode? keyPressed;
		public bool HasKeyPressed(KeyCode key) => keyPressed == key;

		internal void Update()
		{
			var evt = Event.current;
			if (evt == null) return;
			if (evt.type == EventType.Used) return;

#if UNITY_EDITOR
			var sceneView = SceneView.lastActiveSceneView;
			IsIn2DMode = false;
			if (sceneView)
			{
				IsIn2DMode = sceneView.in2DMode;
			}
#endif
			
			switch (evt.type)
			{
				
				case EventType.KeyDown:
					if(evt.keyCode != KeyCode.None)
						keyPressed = evt.keyCode;
					// Debug.Log("KEY PRESSED: "+  keyPressed);
					break;
				case EventType.KeyUp:
					keyPressed = null;
					break;
				
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
				Modifiers = evt.modifiers;
				DeltaWorld = null;
				LastWorldPosition = WorldPosition;
				var camera = Camera.current;
				#if UNITY_EDITOR
				if (sceneView.camera == camera)
				{ 
					WorldPosition = PlaneUtils.GetPointOnCameraPlane(camera, sceneView.pivot, out _);
					if (WorldPosition == null) WorldNormal = null;
					else WorldNormal = (camera.transform.position - WorldPosition).Value.normalized;
				}
				else
				#endif
				{
					WorldPosition = PlaneUtils.GetPointInWorld(camera, out var normal);
					WorldNormal = normal;
				}
				ViewRotation = camera.transform.rotation;
				// Debug.DrawLine(WorldPosition.Value, WorldPosition.Value + WorldNormal.Value, Color.white, 1);
				LastScreenPosition = ScreenPosition;
				var sp = evt.GetCurrentMousePositionBottomTop();
				ScreenPosition = sp;
			}
			
			switch (evt.type)
			{
				case EventType.MouseDown:
					RecordCurrent();
					StartWorldPosition = WorldPosition;
					StartScreenPosition = ScreenPosition;
					Button = (MouseButton)Event.current.button;
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
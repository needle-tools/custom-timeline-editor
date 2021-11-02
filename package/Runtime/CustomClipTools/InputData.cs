﻿using UnityEditor;
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

		public Vector2 ScreenPosition { get; private set; }
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
				var sceneView = SceneView.lastActiveSceneView;
				var cur = Camera.current;
				if (sceneView.camera == cur)
				{
					WorldPosition = PlaneUtils.GetPointOnCameraPlane(cur, sceneView.pivot, out _);
					if (WorldPosition == null) WorldNormal = null;
					else WorldNormal = (cur.transform.position - WorldPosition).Value.normalized;
				}
				else
				{
					WorldPosition = PlaneUtils.GetPointInWorld(cur, out var normal);
					WorldNormal = normal;
				}
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
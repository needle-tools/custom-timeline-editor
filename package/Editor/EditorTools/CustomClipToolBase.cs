using System;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	public abstract class CustomClipToolBase : EditorTool, ICustomClipTool
	{
		private ClipInfoViewModel viewModel;

		public ICustomClip ActiveClip { get; set; }

		public ClipInfoViewModel ViewModel
		{
			set => viewModel = value;
		}

		public abstract bool Supports(Type type);

		protected double CurrentTime => viewModel.clipTime;

		private static Texture2D _toolIcon;
		private VisualElement _toolRootElement;
		private bool _receivedClickDownEvent;
		private bool _receivedClickUpEvent;
		private float currentTime;

		public override void OnActivated()
		{
			_toolRootElement = new VisualElement();
			_toolRootElement.style.width = 200;
			var backgroundColor = EditorGUIUtility.isProSkin
				? new Color(0.21f, 0.21f, 0.21f, 0.8f)
				: new Color(0.8f, 0.8f, 0.8f, 0.8f);
			_toolRootElement.style.backgroundColor = backgroundColor;
			_toolRootElement.style.marginLeft = 10f;
			_toolRootElement.style.marginBottom = 10f;
			_toolRootElement.style.paddingTop = 5f;
			_toolRootElement.style.paddingRight = 5f;
			_toolRootElement.style.paddingLeft = 5f;
			_toolRootElement.style.paddingBottom = 5f;
		}

		public override void OnWillBeDeactivated()
		{
			_toolRootElement?.RemoveFromHierarchy();
		}

		public override void OnToolGUI(EditorWindow window)
		{
			base.OnToolGUI(window);
			if (!(window is SceneView))
				return;
			if (!ToolManager.IsActiveTool(this))
				return;
			if (ActiveClip == null)
			{
				Debug.Log("Clip does not exist, disabling tool");
				ToolManager.RestorePreviousTool();
				return;
			}
			OnToolInput();
		}

		protected abstract void OnToolInput();

		protected static Vector3 GetCurrentMousePositionInScene()
		{
			Vector3 mousePosition = Event.current.mousePosition;
			var placeObject = HandleUtility.PlaceObject(mousePosition, out var newPosition, out var normal);
			return placeObject ? newPosition : HandleUtility.GUIPointToWorldRay(mousePosition).GetPoint(10);
		}
	}
}
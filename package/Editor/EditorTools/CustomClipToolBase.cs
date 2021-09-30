using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	public struct SelectedClip
	{
		public readonly ClipInfoViewModel ViewModel;
		public readonly ICustomClip Clip;

		public SelectedClip(ClipInfoViewModel viewModel, ICustomClip clip)
		{
			ViewModel = viewModel;
			Clip = clip;
		}
	}
	
	public abstract class CustomClipToolBase : EditorTool, ICustomClipTool
	{
		private readonly List<SelectedClip> active = new List<SelectedClip>();

		protected IReadOnlyList<SelectedClip> Active => active;

		public void Add(ClipInfoViewModel vm, ICustomClip clip)
		{
			active.Add(new SelectedClip(vm, clip));
		}

		public void Remove(ICustomClip clip)
		{
			active.RemoveAll(e => e.Clip == clip);
		}

		public void Clear()
		{
			active.Clear();
		}

		public bool ContainsClip(Type clipType) => active.Any(a => a.Clip.GetType() == clipType);

		public abstract bool Supports(Type type);
		
		private static Texture2D _toolIcon;
		private VisualElement _toolRootElement;
		private bool _receivedClickDownEvent;
		private bool _receivedClickUpEvent;

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
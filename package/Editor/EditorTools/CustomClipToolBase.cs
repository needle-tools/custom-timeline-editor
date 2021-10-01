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
		public bool IsNull() => Clip == null;

		public SelectedClip(ClipInfoViewModel viewModel, ICustomClip clip)
		{
			ViewModel = viewModel;
			Clip = clip;
		}
	}
	
	public abstract class CustomClipToolBase : EditorTool, ICustomClipTool
	{
		private new readonly List<SelectedClip> targets = new List<SelectedClip>();

		protected IReadOnlyList<SelectedClip> Targets => targets;

		public void AddTarget(ClipInfoViewModel vm, ICustomClip clip)
		{
			targets.Add(new SelectedClip(vm, clip));
		}

		public void RemoveTarget(ICustomClip clip)
		{
			targets.RemoveAll(e => e.Clip == clip);
		}

		public void RemoveAllTargets()
		{
			targets.Clear();
		}

		public bool HasClipTarget(Type clipType) => targets.Any(a => a.Clip.GetType() == clipType);

		public abstract bool Supports(Type type);
		
		private static Texture2D _toolIcon;
		private VisualElement _toolRootElement;
		private bool _receivedClickDownEvent;
		private bool _receivedClickUpEvent;

		public sealed override void OnActivated()
		{
		}

		public sealed override void OnWillBeDeactivated()
		{
		}

		public sealed override void OnToolGUI(EditorWindow window)
		{
			base.OnToolGUI(window);
			if (!(window is SceneView))
				return;
			if (!ToolManager.IsActiveTool(this))
				return;
			OnToolInput();
		}

		protected virtual void OnAttach(VisualElement element){}
		protected abstract void OnToolInput();

		protected static Vector3 GetCurrentMousePositionInScene()
		{
			Vector3 mousePosition = Event.current.mousePosition;
			var placeObject = HandleUtility.PlaceObject(mousePosition, out var newPosition, out var normal);
			return placeObject ? newPosition : HandleUtility.GUIPointToWorldRay(mousePosition).GetPoint(10);
		}
	}
}
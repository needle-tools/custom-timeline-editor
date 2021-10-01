using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	public readonly struct ToolTarget
	{
		public readonly ClipInfoViewModel ViewModel;
		public readonly ICustomClip Clip;
		public bool IsNull() => Clip == null;

		public ToolTarget(ClipInfoViewModel viewModel, ICustomClip clip)
		{
			ViewModel = viewModel;
			Clip = clip;
		}
	}

	public abstract class CustomClipToolBase : EditorTool, ICustomClipTool
	{
		private new readonly List<ToolTarget> targets = new List<ToolTarget>();
		protected IReadOnlyList<ToolTarget> Targets => targets;

		#region ICustomClipTool
		void ICustomClipTool.AddTarget(ClipInfoViewModel vm, ICustomClip clip)
		{
			targets.Add(new ToolTarget(vm, clip));
		}

		void ICustomClipTool.RemoveTarget(ICustomClip clip)
		{
			targets.RemoveAll(e => e.Clip == clip);
		}

		void ICustomClipTool.RemoveAllTargets()
		{
			targets.Clear();
		}

		bool ICustomClipTool.HasClipTarget(Type clipType) => targets.Any(a => a.Clip.GetType() == clipType);

		private Label debugLabel;
		void ICustomClipTool.Attach(VisualElement el)
		{
			debugLabel ??= new Label(GetType().Name);
			el.Add(debugLabel);
			OnAttach(el);
		}

		void ICustomClipTool.Detach(VisualElement el)
		{
			el.Remove(debugLabel);
			OnDetach(el);
		}

		bool ICustomClipTool.Supports(Type type) => OnSupports(type);
		#endregion

		private static Texture2D _toolIcon;
		private VisualElement _toolRootElement;
		private bool _receivedClickDownEvent;
		private bool _receivedClickUpEvent;

		#region EditorTool
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
			OnTool(window);
		}
		#endregion
		
		
		
		protected abstract bool OnSupports(Type type);
		protected abstract void OnTool(EditorWindow window);


		protected virtual void OnAttach(VisualElement element){}
		protected virtual void OnDetach(VisualElement element){}
		
		
		
		
		
		protected static Vector3 GetCurrentMousePositionInScene()
		{
			Vector3 mousePosition = Event.current.mousePosition;
			var placeObject = HandleUtility.PlaceObject(mousePosition, out var newPosition, out var normal);
			return placeObject ? newPosition : HandleUtility.GUIPointToWorldRay(mousePosition).GetPoint(10);
		}
	}
}
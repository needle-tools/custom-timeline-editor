using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	public abstract class CustomClipToolBase : EditorTool, ICustomClipTool
	{
		private new readonly List<ToolTarget> targets = new List<ToolTarget>();
		protected IReadOnlyList<ToolTarget> Targets => targets;

		#region ICustomClipTool
		void ICustomClipTool.AddTarget(ClipInfoViewModel vm, ICustomClip clip)
		{
			Debug.Log("Add " + clip.Name + "@" + vm.startTime);
			var t = new ToolTarget(vm, clip);
			targets.Add(t);
			OnAddedTarget(t); 
		}

		void ICustomClipTool.RemoveTarget(ICustomClip clip)
		{
			for (var index = targets.Count - 1; index >= 0; index--)
			{
				var t = targets[index];
				if (t.Clip == clip)
				{
					Debug.Log("Remove " + clip.Name + "@" + t.ViewModel.startTime);
					targets.RemoveAt(index);
					OnRemovedTarget(t);
				}
			}
		}

		void ICustomClipTool.RemoveAllTargets()
		{
			foreach (var t in targets)
			{
				OnRemovedTarget(t);
			}
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

		void ICustomClipTool.GetOrCreateSettings(ref ScriptableObject obj)
		{
			OnGetOrCreateSettings(ref obj);
			Settings = obj;
		}
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
			OnInput(window);
		}
		#endregion

		protected ScriptableObject Settings { get; private set; }

		protected virtual void OnGetOrCreateSettings(ref ScriptableObject settings)
		{
		}

		protected virtual void OnAddedTarget(ToolTarget t)
		{
		}

		protected virtual void OnRemovedTarget(ToolTarget t)
		{
		}

		protected abstract bool OnSupports(Type type);
		protected abstract void OnInput(EditorWindow window);


		protected virtual void OnAttach(VisualElement element)
		{
		}

		protected virtual void OnDetach(VisualElement element)
		{
		}


		protected static bool IsIn2DMode => SceneView.lastActiveSceneView?.in2DMode ?? false;

		protected static Vector3 GetCurrentMousePositionInScene()
		{
			return PlaneUtils.GetPointOnPlane(Camera.current, out _, out _, out _);
			// Vector3 mousePosition = Event.current.mousePosition;
			// var placeObject = HandleUtility.PlaceObject(mousePosition, out var newPosition, out var normal);
			// return placeObject ? newPosition : HandleUtility.GUIPointToWorldRay(mousePosition).GetPoint(10);
		}


		protected static void UseEvent()
		{
			if (Event.current == null || Event.current.type == EventType.Used) return;
			// prevent selection box
			GUIUtility.hotControl = 0;
			Event.current.Use();
		}
	}
}
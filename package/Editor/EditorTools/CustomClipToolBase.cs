using System;
using System.Collections.Generic;
using System.Linq;
using Needle.Timeline.Commands;
using Unity.Profiling;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	public abstract class CustomClipToolBase : EditorTool, ICustomClipTool
	{
		private new readonly List<ToolTarget> targets = new List<ToolTarget>();
		public IReadOnlyList<ToolTarget> Targets => targets;

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
			// Settings = obj;
		}

		bool ICustomClipTool.IsValid => this;
		#endregion
		
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
			var isEndInputEvent = false;
			switch (Event.current.type)
			{
				case EventType.MouseUp:
					isEndInputEvent = true;
					break;
			}
			OnInput(window);
			if (isEndInputEvent)
			{
				TimelineBuffer.RequestBufferCurrentInspectedTimeline();
			}
		}
		#endregion


		private void RegisterTargetEvents(ToolTarget t)
		{
			if (t.Clip == null) return;
			t.Clip.RecordingStateChanged += OnRecordingStateChanged;
		}

		private void UnregisterTargetEvents(ToolTarget t)
		{
			if (t.Clip == null) return;
			t.Clip.RecordingStateChanged -= OnRecordingStateChanged;
		}

		protected virtual void OnRecordingStateChanged(IRecordable obj)
		{
			
		}

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

		private ProfilerMarker toolInputMarker;

		protected IInputCommandHandler CommandHandler { get; } = new TimelineInputCommandHandler();

		protected void OnInput(EditorWindow window)
		{
			using (toolInputMarker.Auto())
			{
				_useEventDelayed = false;
				var stage = OnHandleInput();
				if (_useEventDelayed)
					UseEvent();

				switch (stage)
				{
					case InputEventStage.Begin:
						break;
					case InputEventStage.End:
					case InputEventStage.Cancel:
						if (CommandHandler.Count > 0)
						{
							var vm = targets.FirstOrDefault();
							if(vm != null)
								CommandHandler.RegisterCommand(vm.GetTimeCommand());
							CommandHandler.FlushCommands(GetType().Name);
						}
						break;
				}
			}
			
		}


		protected virtual void OnAttach(VisualElement element)
		{
			toolInputMarker = new ProfilerMarker(this.GetType().Name + "." + nameof(OnInput));
		}

		protected virtual void OnDetach(VisualElement element)
		{
		}

		protected virtual InputEventStage OnHandleInput()
		{
			return InputEventStage.Unknown;
		}

		private bool _useEventDelayed;

		protected void UseEventDelayed()
		{
			_useEventDelayed = true;
		}

		private static void UseEvent()
		{
			if (Event.current == null) return;
			switch (Event.current.type)
			{
				case EventType.Used:
				case EventType.Repaint:
				case EventType.Layout:
					return;
			}
			// prevent selection box
			GUIUtility.hotControl = 0;
			Event.current.Use();
		}
	}
}
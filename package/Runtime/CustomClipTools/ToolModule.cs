#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Needle.Timeline
{
	public class ToolInputData
	{
		public Vector3? WorldPosition;
		public Vector3? LastWorldPosition;

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

		public Vector2 ScreenPosition;
		public Vector2 LastScreenPosition;
		public Vector2 ScreenDelta => ScreenPosition - LastScreenPosition;
		public InputEventType Type;

		internal void Update()
		{
			var evt = Event.current;
			if (evt == null) return;
			if (evt.type == EventType.Used) return;
			switch (evt.type)
			{
				case EventType.MouseDown:
					Type = InputEventType.Begin;
					break;
				case EventType.MouseDrag:
					Type = InputEventType.Update;
					break;
				case EventType.MouseUp:
					Type = InputEventType.End;
					break;
			}
			switch (evt.type)
			{
				case EventType.MouseDown:
				case EventType.MouseDrag:
				case EventType.MouseUp:
				case EventType.MouseMove:
					DeltaWorld = null;
					LastWorldPosition = WorldPosition;
					WorldPosition = PlaneUtils.GetPointOnPlane(Camera.current, out _, out _, out _);
					LastScreenPosition = ScreenPosition;
					ScreenPosition = evt.mousePosition;
					break;
			}
		}
	}

	public enum InputEventType
	{
		Begin = 0,
		Update = 1,
		End = 2,
		Cancel = 3
	}

	public interface IToolModule
	{
		bool CanModify(FieldInfo type);
	}

	public abstract class ToolModule : IToolModule
	{
		public abstract bool CanModify(FieldInfo type);
		public bool RequestsInput(ToolInputData input)
		{
			if (input.Type == InputEventType.Update)
			{
				return true;
			}
			return false;
		}

		public bool OnModify()
		{
			Debug.Log("Input on " + this);
			return true;
		}

		#region static
		private static bool modulesInit;
		private static readonly List<ToolModule> modules = new List<ToolModule>();
		public static IReadOnlyList<ToolModule> Modules
		{
			get
			{
				if (!modulesInit)
				{
					modulesInit = true;
					foreach (var mod in RuntimeTypeCache.TypesDerivingFrom<ToolModule>())
					{
						if (mod.IsAbstract || mod.IsInterface) continue;
						if(Activator.CreateInstance(mod) is ToolModule moduleInstance)
							modules.Add(moduleInstance);
					}
				}
				return modules;
			}
		}
		public static void GetModulesSupportingType(FieldInfo type, IList<IToolModule> list)
		{
			list.Clear();
			foreach (var mod in ToolModule.Modules)
			{
				if (mod.CanModify(type))
				{
					list.Add(mod);
				}
			}
		}
		#endregion
	}

	public class DragVector3 : ToolModule
	{
		public override bool CanModify(FieldInfo type)
		{
			return typeof(Vector3).IsAssignableFrom(type.FieldType);
		}

		// public override void OnInput(FieldInfo field, ToolInputData input)
		// {
		// 	// var vec = (Vector3)field.GetValue();
		// }
	}
}
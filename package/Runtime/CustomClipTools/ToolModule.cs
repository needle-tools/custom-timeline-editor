﻿#nullable enable

using System;
using System.Collections;
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
				default:
					Type = InputEventType.Unknown;
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
		Cancel = 3,
		Unknown = 4,
	}

	public interface IToolModule
	{
		bool CanModify(Type type);
	}

	public abstract class ToolModule : IToolModule
	{
		public bool CanModify(Type type)
		{
			return typeof(Vector3).IsAssignableFrom(type);
		}
		public bool WantsInput(ToolInputData input) => input.Type == InputEventType.Begin || input.Type == InputEventType.Update;

		public bool OnModify(ToolInputData input, ref object? value)
		{
			if (value == null) return false;
			var delta = input.DeltaWorld.GetValueOrDefault();
			value = (Vector3)value + delta;
			// switch (target)
			// {
			// 	case IList list:
			// 		
			// 		break;
			// 	case List<Vector3> vecList:
			// 		if (vecList == null) return false;
			// 		for (var index = 0; index < vecList?.Count; index++)
			// 		{
			// 			var entry = vecList[index];
			// 			entry += delta;
			// 			vecList[index] = entry;
			// 		}
			// 		break;
			// 	default:
			// 		foreach (var field in target.GetType().EnumerateFields())
			// 		{
			// 			if (field.FieldType == typeof(Vector3))
			// 			{
			// 				var val = (Vector3)field.GetValue(target);
			// 				val += delta;
			// 				field.SetValue(target, val);
			// 			}
			// 		}
			// 		break;
			// }
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
				if (mod.CanModify(type.FieldType))
				{
					list.Add(mod);
				}
			}
		}
		#endregion
	}

	public class DragVector3 : ToolModule
	{
		// public override bool CanModify(FieldInfo type)
		// {
		// 	return typeof(Vector3).IsAssignableFrom(type.FieldType);
		// }

		// public override void OnInput(FieldInfo field, ToolInputData input)
		// {
		// 	// var vec = (Vector3)field.GetValue();
		// }
	}
}
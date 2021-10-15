#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Animations;

namespace Needle.Timeline
{
	public class InputData
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
	
	public struct ToolData
	{
		public ICustomClip Clip;
		public ICustomKeyframe? Keyframe;
		public Type? ValueType;
		public object? Value;
		public float Time;
	}

	public abstract class ToolModule : IToolModule
	{
		
		public abstract bool CanModify(Type type);
		
		public virtual bool WantsInput(InputData input) => 
			input.Type == InputEventType.Begin || input.Type == InputEventType.Update;

		public virtual bool OnModify(InputData input, ref ToolData toolData)
		{
			return false;
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
		public static void GetModulesSupportingType(Type type, IList<IToolModule> list)
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


	public class SprayModule : ToolModule
	{
		public override bool CanModify(Type type)
		{
			return typeof(Vector3).IsAssignableFrom(type);
		}

		public override bool OnModify(InputData input, ref ToolData toolData)
		{
			if (toolData.Clip.SupportedTypes.Contains(typeof(List<Vector3>)) == false) return false;
			var closestKeyframe = toolData.Keyframe;
			closestKeyframe ??= toolData.Clip.GetClosest(toolData.Time);
			
			if (closestKeyframe == null || Mathf.Abs(closestKeyframe.time - toolData.Time) > .1f)
			{  
				var clipType = toolData.Clip.SupportedTypes.FirstOrDefault();
				if (clipType == null) throw new Exception("Expecting at least one clip type");
				closestKeyframe = toolData.Clip.AddKeyframe(toolData.Time, Activator.CreateInstance(clipType));
			}
			
			if(closestKeyframe != null)
			{
				if (closestKeyframe.value is IList<Vector3> list)
				{
					list.Add(input.WorldPosition.GetValueOrDefault());
					return true;
				}
			}
			
			return false;
		}
	}

	public class DragVector3 : ToolModule
	{
		public override bool CanModify(Type type)
		{
			return typeof(Vector3).IsAssignableFrom(type);
		}

		public override bool OnModify(InputData input, ref ToolData toolData)
		{
			if (toolData.Value is Vector3 vec)
			{
				var dist = Vector3.Distance(input.WorldPosition.Value, (Vector3)toolData.Value); 
				var strength = Mathf.Clamp01(1 - dist);
				if (strength <= 0) return false;
				var delta = input.DeltaWorld.GetValueOrDefault();
				var target = vec + delta;
				toolData.Value = Vector3.Lerp(vec, target, strength);
				return strength > .01f;
			}
			return false;
		}
	}
	public class DragColor : ToolModule
	{
		public override bool CanModify(Type type)
		{
			return typeof(Color).IsAssignableFrom(type); 
		}

		public override bool OnModify(InputData input, ref ToolData toolData)
		{
			if (toolData.Value is Color col)
			{
				// TODO: we need to have access to other fields of custom types, e.g. here we want the position to get the distance
				
				// TODO: figure out how we create new objects e.g. in a list

				Color.RGBToHSV(col, out var h, out var s, out var v);
				h += input.ScreenDelta.x * .001f;
				v += input.ScreenDelta.y * -.001f;
				col = Color.HSVToRGB(h, s, v);
				toolData.Value = Color.Lerp((Color)toolData.Value, col, 1);
				return true;
			}
			return false;
		}
	}
}
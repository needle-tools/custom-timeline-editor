#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Needle.Timeline
{
	public class InputData : IToolData
	{
		public bool IsIn2DMode;
		
		
		public Vector3? WorldPosition { get; private set; }
		public Vector3? LastWorldPosition;
		public Vector3? StartWorldPosition;

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

		public Vector2 ScreenPosition;
		public Vector2 LastScreenPosition;
		public Vector2 StartScreenPosition;
		
		public Vector2 ScreenDelta => ScreenPosition - LastScreenPosition;
		public InputEventType Type;

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

			void RecordCurrent()
			{
				DeltaWorld = null;
				LastWorldPosition = WorldPosition;
				WorldPosition = PlaneUtils.GetPointOnPlane(Camera.current, out _, out _, out _);
				LastScreenPosition = ScreenPosition;
				ScreenPosition = evt.mousePosition;
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
		public int? Index;
		public object? ValueOwner;

		private bool _didSearchPosition;
		private Vector3? _position;
		public Vector3? Position
		{
			get
			{
				if (_didSearchPosition) return _position;
				_didSearchPosition = true;
				return _position = ToolHelpers.TryGetPosition(ValueOwner, Value);
			}
		}
	}

	public abstract class ToolModule : IToolModule
	{
		public bool EventUsed { get; protected set; }

		public abstract bool CanModify(Type type);

		public virtual bool WantsInput(InputData input) =>
			(input.Type == InputEventType.Begin || input.Type == InputEventType.Update)
			&& Event.current.button == 0 && Event.current.modifiers == EventModifiers.None;

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
						if (Activator.CreateInstance(mod) is ToolModule moduleInstance)
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

	public class ModifySelf : ToolModule
	{
		public override bool CanModify(Type type)
		{
			return typeof(IModifySelf).IsAssignableFrom(type);
		}

		public override bool WantsInput(InputData input)
		{
			return true;
		}

		public override bool OnModify(InputData input, ref ToolData toolData)
		{
			if (toolData.Value == null) return false;
			if (toolData.Value is IModifySelf mod)
			{
				return mod.OnInput(input);
			}
			return false;
		}
	}

	public class SprayModule : ToolModule
	{
		public float Radius = 1;

		public override bool WantsInput(InputData input)
		{
			return base.WantsInput(input);
		}

		public override bool CanModify(Type type)
		{
			return typeof(Vector3).IsAssignableFrom(type);
		}

		public override bool OnModify(InputData input, ref ToolData toolData)
		{
			// if (toolData.Clip.SupportedTypes.Contains(typeof(List<Vector3>)) == false) return false;

			if (toolData.Value != null) return false;
			if (input.WorldPosition == null) return false;
			var pos = input.WorldPosition.Value + Random.insideUnitSphere * Radius;
			if (input.IsIn2DMode) pos.z = 0;

			var closestKeyframe = toolData.Keyframe; 
			closestKeyframe ??= toolData.Clip.GetClosest(toolData.Time);

			if (closestKeyframe == null || Mathf.Abs(closestKeyframe.time - toolData.Time) > .1f)
			{
				var clipType = toolData.Clip.SupportedTypes.FirstOrDefault();
				if (clipType == null) throw new Exception("Expecting at least one clip type");
				closestKeyframe = toolData.Clip.AddKeyframe(toolData.Time, Activator.CreateInstance(clipType));
				if (closestKeyframe != null)
					closestKeyframe.time = toolData.Time;
			}

			if (closestKeyframe != null)
			{
				var contentType = closestKeyframe.TryRetrieveKeyframeContentType();

				if (closestKeyframe.value is ICollection<Vector3> list)
				{
					list.Add(pos);
					return true;
				}

				if (closestKeyframe.value is IList col && contentType != typeof(Vector3) && contentType != null)
				{
					var instance = contentType.TryCreateInstance();
					if (instance != null)
					{
						if(instance is IInit i) i.Init(InitStage.InstanceCreated, input);
						var posField = instance.GetType().EnumerateFields().FirstOrDefault(f => f.FieldType == typeof(Vector3));
						posField!.SetValue(instance, pos);
						if (instance is IInit init) init.Init(InitStage.BasicValuesSet, input);
						col.Add(instance);
						return true;
					}
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
			if (toolData.Value is Vector3 vec && input.WorldPosition.HasValue)
			{
				var dist = Vector3.Distance(input.WorldPosition.Value, (Vector3)toolData.Value);
				var strength = Mathf.Clamp01(10 * (1 - dist));
				if (strength <= 0) return false;
				var delta = input.DeltaWorld.GetValueOrDefault();
				var target = vec + delta;
				toolData.Value = Vector3.Lerp(vec, target, strength);
				return true;
			}
			return false;
		}
	}

	public class FloatScaleDrag : ToolModule
	{
		public float Radius = 1;
		
		public override bool CanModify(Type type)
		{
			return typeof(float).IsAssignableFrom(type); 
		}

		public override bool OnModify(InputData input, ref ToolData toolData)
		{
			if (toolData.Value is float vec)
			{
				if (toolData.Position != null)
				{
					var dist = Vector2.Distance(input.StartScreenPosition, input.ToScreenPoint(toolData.Position.Value));
					var strength = Mathf.Clamp01(Radius * 100 - dist);
					if (strength <= 0) return false;
					var delta = input.ScreenDelta.y * 0.01f;
					var target = vec + delta;
					toolData.Value = target;
					return true;
				}
				else
				{
					var delta = -input.ScreenDelta.y * 0.01f;
					var target = vec + delta;
					toolData.Value = target;
					return Mathf.Abs(delta) > .0001f;
				}
			}
			return false;
		}
	}
	
	
	public class SetFloatValue : ToolModule
	{
		public float Value = .1f;
		
		public override bool CanModify(Type type)
		{
			return typeof(float).IsAssignableFrom(type); 
		}

		public override bool OnModify(InputData input, ref ToolData toolData)
		{
			if (toolData.Value is float vec)
			{
				if (toolData.Position != null && input.WorldPosition != null)
				{
					var dist = Vector3.Distance(input.WorldPosition.Value, toolData.Position.Value);
					var strength = Mathf.Clamp01(1 - dist);
					if (strength <= 0) return false;
					toolData.Value = Value;
					return true;
				}
				toolData.Value = Value;
				return true;
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
				float strength = 1;
				if (toolData.Position != null && input.StartWorldPosition != null)
				{
					var dist = Vector3.Distance(input.StartWorldPosition.Value, toolData.Position.Value);
					strength = Mathf.Clamp01(2 - dist);
					if (strength <= 0.001f) return false;
				}
				
				// TODO: we need to have access to other fields of custom types, e.g. here we want the position to get the distance

				// TODO: figure out how we create new objects e.g. in a list

				Color.RGBToHSV(col, out var h, out var s, out var v);
				h += input.ScreenDelta.x * .005f;
				if((Event.current.modifiers & EventModifiers.Alt) != 0)
					s += input.ScreenDelta.y * -.01f;
				else
					v += input.ScreenDelta.y * -.01f;
				col = Color.HSVToRGB(h, s, v);
				toolData.Value = Color.Lerp((Color)toolData.Value, col, strength);
				return true;
			}
			return false;
		}
	}
}
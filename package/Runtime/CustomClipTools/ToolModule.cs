#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework.Internal.Filters;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace Needle.Timeline
{
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

		internal readonly List<IValueHandler> dynamicFields = new List<IValueHandler>();
		// internal readonly IDictionary<FieldInfo, object> _dynamicFields = new Dictionary<FieldInfo, object>();

		public abstract bool CanModify(Type type);

		public virtual bool WantsInput(InputData input)
		{
			return (input.Stage == InputEventStage.Begin || input.Stage == InputEventStage.Update || input.Stage == InputEventStage.End)
			       && AllowedButton((MouseButton)Event.current.button) && AllowedModifiers(input, Event.current.modifiers);
		}

		public virtual void OnDrawGizmos(InputData input)
		{
			if (input.WorldPosition != null)
			{
				Handles.color = new Color(.5f, .5f, .5f, .5f);
				Handles.DrawWireDisc(input.WorldPosition.Value, input.WorldNormal!.Value, GetRadius());
				Gizmos.color = Color.green;
				GizmoUtils.DrawArrow(input.WorldPosition.Value, input.WorldPosition.Value + input.WorldNormal.Value);
				// Handles.DrawWireDisc(input.WorldPosition.Value, Camera.current.transform.forward, 1);
				// Handles.SphereHandleCap(0, input.WorldPosition.Value,  Quaternion.identity, .2f, EventType.Repaint);
			}
		}

		private bool didSearchRadius;
		private FieldInfo radiusField;
		protected virtual float GetRadius()
		{
			if (radiusField != null) return (float)radiusField.GetValue(this);
			if (didSearchRadius) return .1f;
			didSearchRadius = true; 
			foreach (var field in GetType().GetRuntimeFields())
			{
				if (field.FieldType == typeof(float) && field.Name.Equals("radius", StringComparison.InvariantCultureIgnoreCase))
				{
					radiusField = field;
					break;
				}
			}
			return (float)(radiusField?.GetValue(this) ?? 0.1f);
		}

		protected virtual bool AllowedButton(MouseButton button) => button == 0;
		protected virtual bool AllowedModifiers(InputData data, EventModifiers current) => current == EventModifiers.None;

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
					foreach (var mod in RuntimeTypeCache.GetTypesDerivingFrom<ToolModule>())
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

	public class HandlesModule : ToolModule
	{
		// TODO: how can we delete elements
		
		public override bool CanModify(Type type)
		{
			return typeof(ICustomControls).IsAssignableFrom(type);
		}

		public override bool WantsInput(InputData input)
		{
			return true;
		}

		public override bool OnModify(InputData input, ref ToolData toolData)
		{
			if (toolData.Value == null) return false;
			if (toolData.Value is ICustomControls mod)
			{
				return mod.OnCustomControls(input, this);
			}
			return false;
		}
	}

	public class CallbackHandler
	{
		private readonly object obj;
		private readonly IList col;
		private readonly int index;

		public CallbackHandler(object obj, IList col, int index)
		{
			this.obj = obj;
			this.col = col;
			this.index = index;
		}

		public void Modify<T>(Func<T, T> mod)
		{
			if (!(obj is T t)) return;
			t = mod(t);
			col[index] = t;
		}
	}

	public class SprayModule : ToolModule
	{
		[UnityEngine.Range(0,1)]
		public float Probability = 1;
		public float Radius = 1;
		public int Max = 1000;
		[UnityEngine.Range(0,1)]
		public float Offset = 1;
		public bool OnSurface = false;

		private readonly List<CallbackHandler> _created = new List<CallbackHandler>();

		private readonly Type[] _supportedTypes = { typeof(Vector2), typeof(Vector3) };

		public override bool CanModify(Type type)
		{
			return _supportedTypes.Any(t => t.IsAssignableFrom(type));
		}

		protected void EnsureKeyframe(ref ToolData toolData)
		{
			toolData.Keyframe ??= toolData.Clip.GetClosest(toolData.Time);
			if (toolData.Keyframe == null || Mathf.Abs(toolData.Keyframe.time - toolData.Time) > Mathf.Epsilon)
			{
				var clipType = toolData.Clip.SupportedTypes.FirstOrDefault();
				if (clipType == null) throw new Exception("Expecting at least one clip type");
				toolData.Keyframe = toolData.Clip.AddKeyframe(toolData.Time, Activator.CreateInstance(clipType));
				if (toolData.Keyframe != null)
					toolData.Keyframe.time = toolData.Time;
			}
		}

		private bool didBegin = false;
		public override bool OnModify(InputData input, ref ToolData toolData)
		{
			switch (input.Stage)
			{
				case InputEventStage.Begin:
					if (didBegin) return false;
					didBegin = true;
					_created.Clear();
					break;
				case InputEventStage.Update:
					foreach (var ad in _created)
					{
						ad.Modify<IToolEvents>(i =>
						{
							i.OnToolEvent(ToolStage.InputUpdated, input);
							return i;
						});
					}
					break;
				case InputEventStage.End:
				{
					didBegin = false;
					foreach (var ad in _created)
					{
						ad.Modify<IToolEvents>(i =>
						{
							i.OnToolEvent(ToolStage.InputEnded, input);
							return i;
						});
					}
					_created.Clear();
					return true;
				}
			}
			if (Max > 0 && _created.Count >= Max) return true;
			
			// if (toolData.Clip.SupportedTypes.Contains(typeof(List<Vector3>)) == false) return false;

			if (toolData.Value != null) return false;
			if (input.WorldPosition == null) return false;
			if (Random.value > Probability) return false;

			EnsureKeyframe(ref toolData);
			var closestKeyframe = toolData.Keyframe; 
			
			// IDEA: could created objects have creation-properties that could/should be exposed in ui?
			// or is that something that should be handled in creation callbacks and not be exposed?
			// would be cool if types could react differently to input (e.g. create arrows with velocity or fixed length)
			
			if (closestKeyframe != null)
			{
				var offset = Random.insideUnitSphere * Radius;
				var pos = input.WorldPosition.Value + offset;

				if (OnSurface && input.WorldNormal != null)
				{
					if (Physics.SphereCast(pos, Radius * .5f, -input.WorldNormal.Value, out var hit, Radius * 2f))
						pos = hit.point;
					else
					{
						return true;
					}
				}
				pos += Offset * input.WorldNormal.GetValueOrDefault() * Radius;
				
				if (input.IsIn2DMode) pos.z = 0;
				
				var contentType = closestKeyframe.TryRetrieveKeyframeContentType();

				if (closestKeyframe.value is ICollection<Vector3> list3)
				{
					list3.Add(pos);
					closestKeyframe.RaiseValueChangedEvent();
					return true;
				}
				
				if (closestKeyframe.value is ICollection<Vector2> list2)
				{
					list2.Add(pos);
					closestKeyframe.RaiseValueChangedEvent();
					return true;
				}

				var type = _supportedTypes.FirstOrDefault(t => t.IsAssignableFrom(contentType));
				if (closestKeyframe.value is IList col && contentType != type && contentType != null)
				{
					var instance = contentType.TryCreateInstance();
					if (instance != null)
					{
						if(instance is IToolEvents i) i.OnToolEvent(ToolStage.InstanceCreated, input);
						var posField = instance.GetType().EnumerateFields().FirstOrDefault(f =>
							_supportedTypes.Any(e => e.IsAssignableFrom(f.FieldType)));// f.FieldType == type);
						posField!.SetValue(instance, pos.Cast(posField.FieldType));
						if (instance is IToolEvents init) init.OnToolEvent(ToolStage.BasicValuesSet, input);
						col.Add(instance);
						closestKeyframe.RaiseValueChangedEvent();
						if (instance is IToolEvents) 
							_created.Add(new CallbackHandler(instance, col, col.Count-1));
						return true;
					}
				}
			}

			return false;
		}
	}
	
	public class IntModule : ToolModule
	{
		public override bool CanModify(Type type)
		{
			return typeof(int).IsAssignableFrom(type) || typeof(Enum).IsAssignableFrom(type);
		}

		public override bool OnModify(InputData input, ref ToolData toolData)
		{
			foreach (var e in dynamicFields)
			{
				Debug.Log("PAINT WITH " + e.GetValue());
			}
			return base.OnModify(input, ref toolData);
		}
	}
	

	public class DragVector3 : ToolModule
	{
		public float Radius = 1;
		
		public override bool CanModify(Type type)
		{
			return typeof(Vector3).IsAssignableFrom(type);
		}

		public override bool OnModify(InputData input, ref ToolData toolData)
		{
			if (toolData.Value is Vector3 vec && input.WorldPosition.HasValue)
			{
				var dist = Vector3.Distance(input.WorldPosition.Value, (Vector3)toolData.Value);
				var strength = Mathf.Clamp01(2 * (Radius - dist)/Radius);
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
			if (toolData.Value is float)
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
		public float Radius = 1;
		[UnityEngine.Range(0.01f, 3)]
		public float Falloff = 1;
		
		public override bool CanModify(Type type)
		{
			return typeof(Color).IsAssignableFrom(type);
		}

		protected override bool AllowedButton(MouseButton button)
		{
			return button == MouseButton.LeftMouse || button == MouseButton.RightMouse;
		}

		protected override bool AllowedModifiers(InputData data, EventModifiers current)
		{
			return current == EventModifiers.None; 
		}

		public override bool OnModify(InputData input, ref ToolData toolData)
		{
			if (toolData.Value is Color col)
			{
				float strength = 1;
				if (toolData.Position != null && input.StartWorldPosition != null)
				{
					var dist = Vector3.Distance(input.StartWorldPosition.Value, toolData.Position.Value);
					strength = Mathf.Clamp01(((Radius - dist)/Radius)/Falloff);
					if (strength <= 0.001f) return false;
				}
				
				// TODO: we need to have access to other fields of custom types, e.g. here we want the position to get the distance

				// TODO: figure out how we create new objects e.g. in a list

				Color.RGBToHSV(col, out var h, out var s, out var v);
				h += input.ScreenDelta.x * .005f;
				if((Event.current.button == (int)MouseButton.RightMouse))
					s += input.ScreenDelta.y * .01f;
				else
					v += input.ScreenDelta.y * .01f;
				col = Color.HSVToRGB(h, s, v);
				toolData.Value = Color.Lerp((Color)toolData.Value, col, strength);
				return true;
			}
			return false;
		}
	}
}
#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

// ReSharper disable ReplaceWithSingleAssignment.False

namespace Needle.Timeline
{
	public interface IToolModule
	{
		bool CanModify(Type type);
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

	public interface IToolInputEntryCallback
	{
		void NotifyInputEvent(ToolStage stage, InputData data);
	}

	public class ListEntryCallbackHandler : IToolInputEntryCallback
	{
		private readonly object obj;
		private readonly IList list;
		private readonly int index;

		public ListEntryCallbackHandler(object obj, IList list, int index)
		{
			this.obj = obj;
			this.list = list;
			this.index = index;
		}

		public void NotifyInputEvent(ToolStage stage, InputData data)
		{
			if (obj is IToolEvents evt)
			{
				evt.OnToolEvent(stage, data);
				list[index] = evt;
			}
		}
	}

	public abstract class BasicProducerModule : ToolModule
	{
		private bool _didBegin;
		private uint _producedCount;
		private readonly List<IToolInputEntryCallback> _created = new List<IToolInputEntryCallback>();
		private readonly List<ICustomKeyframe> _keyframesChanged = new List<ICustomKeyframe>();


		public override bool CanModify(Type type)
		{
			return SupportedTypes.Any(t => t.IsAssignableFrom(type));
		}

		protected abstract IEnumerable<Type> SupportedTypes { get; }

		public override bool OnModify(InputData input, ref ToolData toolData)
		{
			Debug.Log("");
			switch (input.Stage)
			{
				case InputEventStage.Begin:
					if (_didBegin) return false;
					_didBegin = true;
					_created.Clear();
					_producedCount = 0;
					break;
				case InputEventStage.Update:
					foreach (var ad in _created)
					{
						ad.NotifyInputEvent(ToolStage.InputUpdated, input);
					}
					break;
				case InputEventStage.End:
				{
					_didBegin = false;
					foreach (var ad in _created)
					{
						ad.NotifyInputEvent(ToolStage.InputEnded, input);
					}
					_created.Clear();
					_producedCount = 0;
					return true;
				}
			}

			if (toolData.Value != null) return false;
			if (input.WorldPosition == null) return false;

			// IDEA: could created objects have creation-properties that could/should be exposed in ui?
			// or is that something that should be handled in creation callbacks and not be exposed?
			// would be cool if types could react differently to input (e.g. create arrows with velocity or fixed length)


			// currently we have multiple ways to do the same thing. The ModularTool does run if we have no keyframe, it runs for a keyframe and also for every entry in a list
			// think about how to design this.... we need a unified way to produce and modify keyframes
			// it must work for
			// - single type fields
			// - lists of types
			// - fields within single types + within types in collections
			// it should be able to create new keyframes, modify existing keyframes and also on the fly copy keyframes when dt is too big (or any other reason)
			// it would be great if a module could expose fields to the ui that would be applied to fields when creating new instances if a type allows that (e.g. initialize a new instance with some default values or paint multiple value set from within the UI)


			_keyframesChanged.Clear();
			foreach (var keyframe in GetKeyframes(toolData))
			{
				if (keyframe == null) continue;
				var contentType = keyframe.TryRetrieveKeyframeContentType();
				if (contentType == null) throw new Exception("Failed getting content type");
				var context = new ToolContext()
				{
					Value = keyframe.value,
				};
				var supportedType = SupportedTypes.FirstOrDefault(s => contentType.IsAssignableFrom(s));
				context.SupportedType = supportedType;
				context.ContentType = contentType;

				var didRun = false;

				if (ProduceValues(input, context))
					didRun = true;

				// modify values
				if (ModifyValues(input, context))
					didRun = true;

				if (EraseValues(input, context))
					didRun = true;

				if (didRun) _keyframesChanged.Add(keyframe);
			}

			foreach (var kf in _keyframesChanged)
			{
				kf.RaiseValueChangedEvent();
			}

			return _keyframesChanged.Count > 0;
		}

		protected virtual IEnumerable<ICustomKeyframe?> GetKeyframes(ToolData toolData)
		{
			var keyframe = toolData.Clip.GetClosest(toolData.Time);
			if (!IsCloseKeyframe(toolData, keyframe))
			{
				yield return CreateAndAddNewKeyframe(toolData);
			}
			yield return keyframe;
		}

		protected bool IsCloseKeyframe(ToolData toolData, ICustomKeyframe? keyframe) => keyframe != null && Mathf.Abs(keyframe.time - toolData.Time) <= Mathf.Epsilon;
		
		protected ICustomKeyframe? CreateAndAddNewKeyframe(ToolData toolData)
		{
			var clipType = toolData.Clip.SupportedTypes.FirstOrDefault();
			if (clipType == null) throw new Exception("Expecting at least one clip type");
			var keyframe = toolData.Clip.AddKeyframe(toolData.Time, Activator.CreateInstance(clipType));
			if (keyframe != null)
				keyframe.time = toolData.Time;
			return keyframe;
		}

		private bool EraseValues(InputData input, ToolContext toolContext)
		{
			var didRun = false;
			if (toolContext.List != null)
			{
				for (var index = toolContext.List.Count - 1; index >= 0; index--)
				{
					var e = toolContext.List[index];
					var context = new DeleteContext(e, index);
					if (!OnDeleteValue(input, ref context))
						break;
					if (context.Deleted)
					{
						didRun = true;
						toolContext.List.RemoveAt(index);
					}
				}
			}
			else if (toolContext.Value != null)
			{
				var context = new DeleteContext(toolContext.Keyframe.value);
				if (!OnDeleteValue(input, ref context)) return false;
				if (context.Deleted)
				{
					didRun = true;
					toolContext.Keyframe.value = null;
				}
			}

			return didRun;
		}

		protected virtual bool OnDeleteValue(InputData input, ref DeleteContext context)
		{
			return false;
		}

		private bool ProduceValues(InputData input, ToolContext toolContext)
		{
			var type = toolContext.ContentType ?? toolContext.Value?.GetType();
			if (type == null) throw new Exception("Failed finding keyframe type that can be assigned: " + toolContext.Value);

			var context = new ProduceContext(_producedCount, toolContext.List != null ? new uint?() : 1);

			var didProduceValue = false;
			foreach (var res in OnProduceValues(input, context))
			{
				if (!res.Success) continue;
				var value = res.Value;
				object instance;
				if (toolContext.SupportedType != null)
				{
					instance = value;
				}
				else
				{
					instance = type.TryCreateInstance() ?? throw new Exception("Failed creating instance of " + toolContext.ContentType + ", Module: " + this);
				}

				// the content type is a field inside a type
				if (instance is IToolEvents i) i.OnToolEvent(ToolStage.InstanceCreated, input);

				if (toolContext.SupportedType == null)
				{
					var matchingField = instance.GetType().EnumerateFields().FirstOrDefault(f =>
						SupportedTypes.Any(e => e.IsAssignableFrom(f.FieldType)));
					if (matchingField == null)
					{
						Debug.Log("Failed producing matching type or field not found... this is most likely a bug");
						return false;
					}
					matchingField.SetValue(instance, value.Cast(matchingField.FieldType));
				}
				if (instance is IToolEvents init) init.OnToolEvent(ToolStage.BasicValuesSet, input);
				if (toolContext.List != null)
				{
					toolContext.List.Add(instance);
				}
				++_producedCount;
				didProduceValue = true;
				if (instance is IToolEvents)
					_created.Add(new ListEntryCallbackHandler(instance, toolContext.List, toolContext.List.Count - 1));
			}
			return didProduceValue;
		}

		protected virtual IEnumerable<ProducedValue> OnProduceValues(InputData input, ProduceContext context)
		{
			yield break;
		}


		private bool ModifyValues(InputData input, ToolContext toolContext)
		{
			var didRun = false;
			if (toolContext.SupportedType != null)
			{
				var list = toolContext.List;
				if (toolContext.List?.Count <= 0) return false;
				var context = new ModifyContext(null, null);
				if (list != null)
				{
					for (var index = 0; index < list.Count; index++)
					{
						var value = list[index];
						if (!OnModifyValue(input, context, ref value))
							break;
						list[index] = value;
						didRun = true;
					}
				}
			}
			else
			{
				var list = toolContext.List;
				if (list != null)
				{
					if (list.Count <= 0) return false;
					object? instance = default;
					for (var index = 0; index < list.Count; index++)
					{
						instance = list[index];
						if (instance != null) break;
					}
					if (instance != null)
					{
						var matchingField = instance.GetType().EnumerateFields().FirstOrDefault(f =>
							SupportedTypes.Any(e => e.IsAssignableFrom(f.FieldType)));
						if (matchingField == null)
						{
							Debug.Log("Failed producing matching type or field not found... this is most likely a bug");
							return false;
						}
						var context = new ModifyContext(null, null);
						for (var index = 0; index < list.Count; index++)
						{
							var entry = list[index];
							var value = matchingField.GetValue(entry);
							if (!OnModifyValue(input, context, ref value))
								break;
							matchingField.SetValue(entry, value.Cast(matchingField.FieldType));
							list[index] = entry;
							didRun = true;
						}
					}
				}
			}
			return didRun;
		}

		protected virtual bool OnModifyValue(InputData input, ModifyContext context, ref object value)
		{
			return false;
		}
	}

	public struct ToolContext
	{
		public IValueOwner Keyframe;
		public object? Value;
		public IList? List => Value as IList;
		public Type? ContentType;
		public Type? SupportedType;
	}

	public struct DeleteContext
	{
		public readonly object Value;
		public readonly int? Index;
		public bool Deleted;

		public DeleteContext(object value, int? index = null)
		{
			Value = value;
			Deleted = false;
			Index = index;
		}
	}

	public readonly struct ModifyContext
	{
		public readonly IValueProvider? ViewValue;
		public readonly string? Name;

		public ModifyContext(string? name = null, IValueProvider? viewValue = null)
		{
			ViewValue = viewValue;
			Name = name;
		}
	}

	public readonly struct ProduceContext
	{
		public readonly uint Count;
		public readonly uint? MaxExpected;

		public ProduceContext(uint count, uint? maxExpected)
		{
			Count = count;
			this.MaxExpected = maxExpected;
		}
	}

	public readonly struct ProducedValue
	{
		public readonly object Value;
		public readonly bool Success;

		public ProducedValue(object value, bool success)
		{
			Value = value;
			Success = success;
		}
	}

	public class SprayProducer : BasicProducerModule
	{
		[Range(0, 1)] public float Probability = 1;
		public float Radius = 1;
		public int Max = 1000;
		[Range(0, 1)] public float Offset = 1;
		public bool OnSurface = false;
		public bool AllKeyframes = false;

		protected override IEnumerable<Type> SupportedTypes { get; } = new[] { typeof(Vector3), typeof(Vector2) };

		protected override IEnumerable<ICustomKeyframe?> GetKeyframes(ToolData toolData)
		{
			foreach (var kf in base.GetKeyframes(toolData))
				yield return kf;
			
			if (AllKeyframes)
			{
				foreach (var kf in toolData.Clip.Keyframes)
				{
					var keyframe = kf as ICustomKeyframe;
					if (IsCloseKeyframe(toolData, keyframe)) continue;
					yield return keyframe;
				}
			}
		}

		protected override IEnumerable<ProducedValue> OnProduceValues(InputData input, ProduceContext context)
		{
			if (input.HasKeyPressed(KeyCode.M)) yield break;
			if (context.Count >= Max) yield break;

			var offset = Random.insideUnitSphere * Radius;
			var pos = input.WorldPosition.Value + offset;

			if (OnSurface && input.WorldNormal != null)
			{
				if (Physics.SphereCast(pos, Radius * .5f, -input.WorldNormal.Value, out var hit, Radius * 2f))
					pos = hit.point;
				else
				{
					yield return new ProducedValue(pos, true);
				}
			}
			pos += Offset * input.WorldNormal.GetValueOrDefault() * Radius;

			if (input.IsIn2DMode) pos.z = 0;
			yield return new ProducedValue(pos, true);
		}
	}

	public class DragPosition : BasicProducerModule
	{
		public float Radius = 1;

		protected override IEnumerable<Type> SupportedTypes { get; } = new[] { typeof(Vector3), typeof(Vector2) };

		protected override bool OnDeleteValue(InputData input, ref DeleteContext context)
		{
			var vec = (Vector3)context.Value.Cast(typeof(Vector3));
			if (Vector3.Distance(vec, input.WorldPosition.Value) < Radius)
			{
				context.Deleted = true;
			}
			return true;
		}

		protected override bool OnModifyValue(InputData input, ModifyContext context, ref object value)
		{
			var vec = (Vector3)value.Cast(typeof(Vector3));
			if (Vector3.Distance(vec, input.WorldPosition.Value) < Radius)
			{
				vec += input.DeltaWorld.Value;
				value = vec;
				return true;
			}
			return true;
		}
	}

	public class IntModule : ToolModule
	{
		public float Radius = 1;

		public override bool CanModify(Type type)
		{
			return typeof(int).IsAssignableFrom(type) || typeof(Enum).IsAssignableFrom(type);
		}

		public override bool OnModify(InputData input, ref ToolData toolData)
		{
			foreach (var e in dynamicFields)
			{
				var dist = Vector3.Distance(input.WorldPosition.GetValueOrDefault(), (Vector3)toolData.Position.GetValueOrDefault());
				if (dist < Radius)
				{
					// Debug.Log("PAINT WITH " + e.GetValue() + " on " + toolData.ValueType);
					toolData.Value = e.GetValue();
					return true;
				}
			}
			return base.OnModify(input, ref toolData);
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
		[UnityEngine.Range(0.01f, 3)] public float Falloff = 1;

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
					strength = Mathf.Clamp01(((Radius - dist) / Radius) / Falloff);
					if (strength <= 0.001f) return false;
				}

				// TODO: we need to have access to other fields of custom types, e.g. here we want the position to get the distance

				// TODO: figure out how we create new objects e.g. in a list

				Color.RGBToHSV(col, out var h, out var s, out var v);
				h += input.ScreenDelta.x * .005f;
				if ((Event.current.button == (int)MouseButton.RightMouse))
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
#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	public interface IBindsFields
	{
		bool AllowBinding { get; }
		internal List<IViewFieldBinding> Bindings { get; }
	}

	public abstract class ToolModule : IToolModule, IBindsFields
	{
		/// <summary>
		/// the cache of interpolators, may also contain null entries if no interpolator for a type was found
		/// </summary>
		private static readonly Dictionary<Type, IInterpolatable?> interpolateCache = new Dictionary<Type, IInterpolatable?>();

		protected bool TryGetInterpolatable(Type type, out IInterpolatable? i)
		{
			if (interpolateCache.TryGetValue(type, out i))
				return i != null;
			if (InterpolatorBuilder.TryFindInterpolatable(type, out i, true))
			{
				interpolateCache.Add(type, i);
				return true;
			}
			interpolateCache.Add(type, null);
			return false;
		}

		protected bool IsAnyBindingEnabled() => ((IBindsFields)this).Bindings.Any(b => b.Enabled);
		protected bool IsEnabled(MemberInfo member)
		{
			var bindings = ((IBindsFields)this).Bindings;
			foreach (var mem in bindings)
			{
				if (mem.Matches(member))
					return mem.Enabled;
			}
			return false;
		}

		public bool AllowBinding { get; protected set; } = false;
		List<IViewFieldBinding> IBindsFields.Bindings { get; } = new List<IViewFieldBinding>();
		private static ProfilerMarker _bindingMarker = new ProfilerMarker("ToolModule.ApplyBinding");

		protected virtual bool ApplyBinding(object obj, float weight, MemberInfo? member = null)
		{
			if (!AllowBinding) return false;
			if (weight <= 0) return false;
			var appliedAny = false;
			using (_bindingMarker.Auto())
			{
				// Debug.Log(weight);
				var bindings = ((IBindsFields)this).Bindings;
				foreach (var field in bindings)
				{
					if (!field.Enabled) continue;
					if (member != null && !field.Equals(member)) continue;
					appliedAny = true;
					var viewValue = field.ViewValue.GetValue();

					if (viewValue != null)
					{
						var type = viewValue.GetType();
						if (TryGetInterpolatable(type, out var interpolatable))
						{
							if (interpolatable != null)
								interpolatable.Interpolate(ref viewValue, field.GetValue(obj), viewValue, weight);
						}
					}

					field.SetValue(obj, viewValue);
				}
			}
			return appliedAny;
		}

		public virtual bool CanModify(Type type)
		{
			if (OnTestCanModify(type)) return true;
			if (type.IsGenericType && typeof(IList).IsAssignableFrom(type))
			{
				var par = type.GetGenericArguments().FirstOrDefault();
				if(par != null)
				{
					if (OnTestCanModify(par))
						return true;
				}
			}
			foreach (var field in type.EnumerateFields())
			{
				if (OnTestCanModify(field.FieldType))
				{
					return true;
				}
			}
			return false;
		}

		protected virtual bool OnTestCanModify(Type type)
		{
			return false;
		}

		public virtual void Reset()
		{
		}

		public virtual bool WantsInput(InputData input)
		{
			return (input.Stage == InputEventStage.Begin || input.Stage == InputEventStage.Update || input.Stage == InputEventStage.End)
			       && AllowedButton((MouseButton)Event.current.button) && AllowedModifiers(input, Event.current.modifiers);
		}

		public virtual void OnDrawGizmos(InputData input)
		{
			if (input.WorldPosition != null)
			{
#if UNITY_EDITOR
				Handles.color = new Color(.5f, .5f, .5f, .5f);
				Handles.DrawWireDisc(input.WorldPosition.Value, input.WorldNormal!.Value, GetRadius());
#endif
				Gizmos.color = Color.green;
				GizmoUtils.DrawArrow(input.WorldPosition.Value, input.WorldPosition.Value + input.WorldNormal.Value);
				// Handles.DrawWireDisc(input.WorldPosition.Value, Camera.current.transform.forward, 1);
				// Handles.SphereHandleCap(0, input.WorldPosition.Value,  Quaternion.identity, .2f, EventType.Repaint);
			}
		}

		private bool didSearchRadius;
		private FieldInfo? radiusField;

		public float? Radius
		{
			get
			{
				if (didSearchRadius && radiusField == null) return null;
				return GetRadius();
			}
		}

		protected virtual float GetRadius()
		{
			if (radiusField != null) return (float)radiusField.GetValue(this);
			if (didSearchRadius) return 0;
			didSearchRadius = true;
			foreach (var field in GetType().GetRuntimeFields())
			{
				if (field.FieldType == typeof(float) && field.Name.Equals("radius", StringComparison.InvariantCultureIgnoreCase))
				{
					radiusField = field;
					break;
				}
			}
			if (radiusField != null && radiusField.FieldType == typeof(float))
				return (float)radiusField.GetValue(this);
			return 0;
		}

		protected virtual bool AllowedButton(MouseButton button) => button == 0;
		protected virtual bool AllowedModifiers(InputData data, EventModifiers current) => current == EventModifiers.None;

		public virtual bool OnModify(InputData input, ref ToolData toolData)
		{
			return false;
		}
	}
}
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
		List<IViewFieldBinding> Bindings { get; }
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
		public List<IViewFieldBinding> Bindings { get; }= new List<IViewFieldBinding>();
		private static ProfilerMarker _bindingMarker = new ProfilerMarker("ToolModule.ApplyBinding");

		protected bool ApplyBinding(object obj, float weight, MemberInfo? member = null)
		{
			if (!AllowBinding) return false;
			if (weight <= 0) return false;
			
			var appliedAny = false;
			using (_bindingMarker.Auto())
			{
				// Debug.Log(weight);
				var bindings = ((IBindsFields)this).Bindings;
				var declaringType = obj.GetType();
				foreach (var bind in bindings)
				{
					if (!bind.Enabled) continue;
					if (member != null && !bind.Equals(member))
					{
						continue;
					}
					if (!bind.CanAssign(declaringType)) continue;
					appliedAny = true;
					var viewValue = bind.ViewValue.GetValue();

					var type = viewValue.GetType();
					if (TryGetInterpolatable(type, out var interpolatable))
					{
						if (interpolatable != null)
							interpolatable.Interpolate(ref viewValue, bind.GetValue(obj), viewValue, weight);
					}
					bind.SetValue(obj, viewValue);
				}
			}
			return appliedAny;
		}

		public virtual bool CanModify() => true;

		public virtual bool CanModify(Type type)
		{
			if (OnInternalCanModify(type)) return true;
			if (type.IsGenericType && typeof(IList).IsAssignableFrom(type))
			{
				var par = type.GetGenericArguments().FirstOrDefault();
				if(par != null)
				{
					if (OnInternalCanModify(par))
						return true;
				}
			}
			foreach (var field in type.EnumerateFields())
			{
				if (OnInternalCanModify(field.FieldType))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Is called per type from OnModify, do not attempt to try search for nested types in here,
		/// instead override CanModify
		/// </summary>
		protected virtual bool OnInternalCanModify(Type type)
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
		
		private readonly Color normalColor = new Color(.5f, .5f, .5f, .6f);
		private readonly Color noInputColor = new Color(1f, .1f, .1f, .3f);

		public virtual void OnDrawGizmos(InputData input)
		{
#if UNITY_EDITOR
			if (input.WorldPosition != null)
			{
				var renderActive = CanModify();
				var pos = input.WorldPosition.Value;
				Handles.color = renderActive ? normalColor : noInputColor;
				Handles.DrawWireDisc(pos, input.WorldNormal!.Value, GetRadius(), 1.2f);

				Gizmos.color = Color.gray;
				var target = pos;
				target.y = 0;
				Gizmos.DrawLine(pos, target);
				// target = pos;
				// target.z = 0;
				// Gizmos.DrawLine(pos, target);
				// target = pos;
				// target.x = 0;
				// Gizmos.DrawLine(pos, target);

				// Gizmos.color = Color.green;
				// GizmoUtils.DrawArrow(input.WorldPosition.Value, input.WorldPosition.Value + input.WorldNormal.Value);
				// Handles.DrawWireDisc(input.WorldPosition.Value, Camera.current.transform.forward, 1);
				// Handles.SphereHandleCap(0, input.WorldPosition.Value,  Quaternion.identity, .2f, EventType.Repaint);
			}
#endif
		}

		private bool didSearchRadius;
		private FieldInfo? radiusField;

		public float? RadiusValue
		{
			get
			{
				if (didSearchRadius && radiusField == null) return null;
				return GetRadius();
			}
		}

		private float GetRadius()
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
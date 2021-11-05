using System;
using System.Collections.Generic;
using System.Reflection;
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
		public bool AllowBinding { get; protected set; } = false;
		List<IViewFieldBinding> IBindsFields.Bindings { get; } = new List<IViewFieldBinding>();
		
		protected virtual bool ApplyBoundValues(object obj, float weight)
		{
			if (!AllowBinding) return false;
			if (weight <= 0) return false;
			Debug.Log(weight);
			var appliedAny = false;
			var bindings = ((IBindsFields)this).Bindings;
			foreach (var field in bindings)
			{
				if (!field.Enabled) continue;
				appliedAny = true;
				var ui = field.View.GetValue();

				if (ui != null)
				{
					if (InterpolatorBuilder.TryFindInterpolatable(ui.GetType(), out var i, true))
					{
						i.Interpolate(ref ui, field.GetValue(obj), ui, weight);
					}
				}
				
				field.SetValue(obj, ui);
			}
			return appliedAny;
		}

		public abstract bool CanModify(Type type);
		
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
	}
}
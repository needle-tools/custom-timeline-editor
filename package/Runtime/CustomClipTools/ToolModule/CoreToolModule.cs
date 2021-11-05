﻿#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

// ReSharper disable ReplaceWithSingleAssignment.False

namespace Needle.Timeline
{
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

	public abstract class CoreToolModule : ToolModule
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
			Debug.Log(_didBegin + ", " + input.Stage);
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
			try
			{
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
			}
			catch (Exception e)
			{
				Debug.LogException(e);
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
			var keyframe = toolData.Clip.AddKeyframeWithUndo(toolData.Time, Activator.CreateInstance(clipType));
			if (keyframe != null)
				keyframe.time = toolData.Time;
			return keyframe; 
		}

		private bool EraseValues(InputData input, ToolContext toolContext)
		{
			var didRun = false;
			if (toolContext.List != null) 
			{
				FieldInfo? field = null;
				var triedFindingField = false;
				for (var index = toolContext.List.Count - 1; index >= 0; index--)
				{
					var e = toolContext.List[index];
					if (e == null) continue;
					DeleteContext context;
					if (toolContext.SupportedType == null)
					{
						if (field == null && triedFindingField) break;
						field ??= TryGetMatchingField(e);
						if (field == null)
						{
							Debug.Log("Failed producing matching type or field not found... this is most likely a bug");
							break;
						}
						var value = field.GetValue(e);
						context = new DeleteContext(value, index); 
					}
					else context = new DeleteContext(e, index);
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
					var matchingField = TryGetMatchingField(instance);
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

		private FieldInfo? TryGetMatchingField(object obj)
		{
			var matchingField = obj.GetType().EnumerateFields().FirstOrDefault(
				f => SupportedTypes.Any(e => e.IsAssignableFrom(f.FieldType)));
			return matchingField;
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
					// try find first instance in list that we can modify
					for (var index = 0; index < list.Count; index++)
					{
						instance = list[index];
						if (instance != null) break;
					}
					// check if we found a instance
					if (instance == null) return didRun;
					
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

}
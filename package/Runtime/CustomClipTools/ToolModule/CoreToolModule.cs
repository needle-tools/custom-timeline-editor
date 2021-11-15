#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Needle.Timeline.Commands;
using Unity.Profiling;
using UnityEngine;
using Debug = UnityEngine.Debug;

// ReSharper disable ReplaceWithSingleAssignment.False

namespace Needle.Timeline
{
	public enum ToolInputResult
	{
		/// <summary>
		/// Tool did modify value and value should be updated
		/// </summary>
		Success = 0,
		/// <summary>
		/// Tool did not affect value and just continue
		/// </summary>
		Failed = 1,
		/// <summary>
		/// Tool failed and should stop affecting any other value
		/// </summary>
		AbortFurtherProcessing = 2,
		/// <summary>
		/// Tool wants to collect this value to be captured and called with a list of all captured values at the end
		/// </summary>
		CaptureForFinalize = 5,
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

	public abstract class CoreToolModule : ToolModule
	{
		private bool _didBegin;
		private uint _producedCount;
		private readonly List<IToolInputEntryCallback> _created = new List<IToolInputEntryCallback>();
		private readonly List<ICustomKeyframe> _keyframesChanged = new List<ICustomKeyframe>();
		private static readonly ProfilerMarker _eraseMarker = new ProfilerMarker("CoreToolModule.Erase");
		private static readonly ProfilerMarker _modifyMarker = new ProfilerMarker("CoreToolModule.Modify");
		private static readonly ProfilerMarker _produceMarker = new ProfilerMarker("CoreToolModule.Produce");

		public override bool CanModify(Type type)
		{
			// var isList = typeof(IList).IsAssignableFrom(type);
			foreach (var sup in SupportedTypes)
			{
				if (sup.IsAssignableFrom(type)) return true;
				// if (isList)
				// {
				// 	return true;
				// }
			}

			return false;
		}

		/// <summary>
		/// Return empty array to support any type
		/// </summary>
		protected abstract IList<Type> SupportedTypes { get; }

		public override bool OnModify(InputData input, ref ToolData toolData)
		{
			switch (input.Stage)
			{
				case InputEventStage.Begin:
					if (_didBegin) return false;
					_didBegin = true;
					_created.Clear();
					_producedCount = 0;
					// _commands.Clear();
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
					if (contentType == null) ThrowHelper.Throw("Failed getting content type");
					var context = new ToolContext()
					{
						Value = keyframe.value,
					};
					var supportedType = TryGetSupportedType(contentType); // SupportedTypes.FirstOrDefault(s => contentType!.IsAssignableFrom(s));
					context.MatchingType = supportedType;
					context.ContentType = contentType;

					var didRun = false;
					
					EditKeyframeValue? edit = default;
					if (!toolData.CommandHandler.HasCommand(kf => kf is EditKeyframeValue ed && ed.IsKeyframe(keyframe)))
						edit = new EditKeyframeValue(keyframe);
					
					using (_produceMarker.Auto())
					{
						if (ProduceValues(input, context, ref toolData))
							didRun = true;
					}

					using (_modifyMarker.Auto())
					{
						// modify values
						if (ModifyValues(input, toolData, context))
							didRun = true;
					}

					using (_eraseMarker.Auto())
					{
						if (EraseValues(input, context))
							didRun = true;
					}
					
					if (didRun)
					{
						_keyframesChanged.Add(keyframe);
						
						// only register editing command if tool did do anything
						if(edit != null)
							toolData.CommandHandler.RegisterCommand(edit);
					}
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

		private Type? TryGetSupportedType(Type contentType)
		{
			if (SupportedTypes.Count <= 0) return contentType;
			return SupportedTypes.FirstOrDefault(s => contentType.IsAssignableFrom(s));
		}

		protected virtual IEnumerable<ICustomKeyframe?> GetKeyframes(ToolData toolData)
		{
			var keyframe = toolData.Clip.GetClosest(toolData.Time);
			yield return keyframe;
		}

		protected bool IsCloseKeyframe(ToolData toolData, ICustomKeyframe? keyframe) =>
			keyframe != null && Mathf.Abs(keyframe.time - toolData.Time) <= Mathf.Epsilon;

		protected ICustomKeyframe? CreateAndAddNewKeyframe(ToolData toolData)
		{
			var clipType = toolData.Clip.SupportedTypes.FirstOrDefault();
			if (clipType == null) ThrowHelper.Throw("Expecting at least one clip type");
			var keyframe = toolData.Clip.AddKeyframeWithUndo(toolData.Time, Activator.CreateInstance(clipType!));
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
					if (toolContext.MatchingType == null)
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
					var res = OnDeleteValue(input, ref context);
					if (res == ToolInputResult.AbortFurtherProcessing) return didRun;
					if (res != ToolInputResult.Success) continue;
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
				var res = OnDeleteValue(input, ref context);
				if (res == ToolInputResult.AbortFurtherProcessing) return didRun;
				if (res == ToolInputResult.Success && context.Deleted)
				{
					didRun = true;
					toolContext.Keyframe.value = null;
				}
			}

			return didRun;
		}

		protected virtual ToolInputResult OnDeleteValue(InputData input, ref DeleteContext context)
		{
			return ToolInputResult.Failed;
		}

		private bool ProduceValues(InputData input, ToolContext toolContext, ref ToolData toolData)
		{
			var type = toolContext.ContentType ?? toolContext.Value?.GetType();
			if (type == null) ThrowHelper.Throw("Failed finding keyframe type that can be assigned: " + toolContext.Value);
			
			var context = new ProduceContext(_producedCount, toolContext.List != null ? new uint?() : 1, type!, toolData.Object);
			var didProduceValue = false;
			foreach (var res in OnProduceValues(input, context))
			{
				if (!res.Success) continue;
				var value = res.Value;
				object? instance;
				if (toolContext.MatchingType != null)
				{
					instance = value;
				}
				else
				{
					instance = type!.TryCreateInstance();
					if (instance == null)
						ThrowHelper.Throw("Failed creating instance of " + toolContext.ContentType + ", Module: " + this);
				}


				if(instance != null)
					ApplyBinding(instance, res.Weight);

				// the content type is a field inside a type
				if (instance is IToolEvents i) i.OnToolEvent(ToolStage.InstanceCreated, input);

				if (toolContext.MatchingType == null && instance != null)
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

		private bool ModifyValues(InputData input, ToolData data, ToolContext toolContext)
		{
			var didRun = false;
			if (toolContext.MatchingType != null)
			{
				var list = toolContext.List;
				if (toolContext.List?.Count <= 0) return false;
				if (list != null)
				{
					for (var index = 0; index < list.Count; index++)
					{
						var value = list[index];
						var context = new ModifyContext(value, index, 0);
						var res = OnModifyValue(input, ref context, ref value);
						if (res == ToolInputResult.AbortFurtherProcessing)
							break;
						if (res != ToolInputResult.Success) continue;
						ApplyBinding(value, context.Weight);
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

					// get all the fields in a type that are processable
					// e.g. a direction has two position fields that we want to check against
					IEnumerable<FieldInfo> matchingFields;
					if (SupportedTypes.Count > 0)
						matchingFields = instance.GetType().EnumerateFields(f => SupportedTypes.Any(e => e.IsAssignableFrom(f.FieldType)));
					else // supported types is empty assume it supports all types
						matchingFields = instance.GetType().EnumerateFields();

					var aborted = false;
					BeforeModifyList();
					for (var index = 0; index < list.Count; index++)
					{
						if (aborted) break;
						var entry = list[index];
						var memberIndex = 0;
						foreach (var matchingField in matchingFields)
						{
							using (_modifyLoopFieldsMarker.Auto())
							{
								var context = new ModifyContext(entry, index, memberIndex);
								memberIndex++;
								var value = matchingField.GetValue(entry);
								var res = OnModifyValue(input, ref context, ref value);
								if (res == ToolInputResult.AbortFurtherProcessing)
								{
									aborted = true;
									break;
								}
								if (res == ToolInputResult.CaptureForFinalize)
								{
									CaptureEntry(matchingField, context, value);
									continue;
								}
								if (res != ToolInputResult.Success)
								{
									continue;
								}
								matchingField.SetValue(entry, value.Cast(matchingField.FieldType));
								ApplyBinding(entry, context.Weight, matchingField);
								list[index] = entry;
								didRun = true;
							}
						}
					}
					if (AfterModifyList(input, list))
						didRun = true;
				}
			}
			return didRun;
		}

		private readonly List<FieldInfo> capturedFieldsCache = new List<FieldInfo>();
		private readonly List<CapturedModifyContext> capturedContexts = new List<CapturedModifyContext>();

		private void BeforeModifyList()
		{
			capturedFieldsCache.Clear();
			capturedContexts.Clear();
		}

		private void CaptureEntry(FieldInfo field, ModifyContext context, object value)
		{
			capturedFieldsCache.Add(field);
			capturedContexts.Add(new CapturedModifyContext(context, value, capturedContexts.Count));
		}

		private bool AfterModifyList(InputData input, IList list)
		{
			if (capturedContexts.Count <= 0) return false;

			var res = OnModifyCaptured(input, capturedContexts);
			if (res == ToolInputResult.Success)
			{
				for (var index = 0; index < capturedContexts.Count; index++)
				{
					var cap = capturedContexts[index];
					var field = capturedFieldsCache[cap.Index];
					var value = cap.Value;
					field.SetValue(cap.Context.Object, value.Cast(field.FieldType));
					list[cap.Context.Index] = cap.Context.Object;
				}
			}

			capturedFieldsCache.Clear();
			capturedContexts.Clear();
			return true;
		}
		

		private static ProfilerMarker _modifyLoopFieldsMarker = new ProfilerMarker("Modify.LoopFields");

		protected virtual ToolInputResult OnModifyValue(InputData input, ref ModifyContext context, ref object value)
		{
			return ToolInputResult.Failed;
		}

		protected virtual ToolInputResult OnModifyCaptured(InputData input, List<CapturedModifyContext> captured)
		{
			return ToolInputResult.Failed;
		}
	}

	public struct ToolContext
	{
		public IValueOwner Keyframe;
		public object? Value;
		public IList? List => Value as IList;
		public Type? ContentType;
		public Type? MatchingType;
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

	public struct ModifyContext
	{
		/// <summary>
		/// The owner of the original value (e.g. if modification happens on a field we need this)
		/// </summary>
		public readonly object Object;
		public float Weight;
		/// <summary>
		/// The index in the original collection (if any)
		/// </summary>
		public readonly int Index;
		public readonly int MemberIndex;

		/// <summary>
		/// Can be set to pass data from capturing stage to processing stage
		/// </summary>
		public object AdditionalData;

		public ModifyContext(object target, int index, int memberIndex)
		{
			Object = target;
			Weight = 1;
			this.Index = index;
			AdditionalData = null;
			MemberIndex = memberIndex;
		}
	}

	public struct CapturedModifyContext
	{
		public readonly ModifyContext Context;
		/// <summary>
		/// The value to be modified
		/// </summary>
		public object Value;
		/// <summary>
		/// The index of this capture in the capture list. We need this to get the matching field again and be safe if tool removes items or shuffles them around
		/// </summary>
		public readonly int Index;

		public CapturedModifyContext(ModifyContext context, object value, int index)
		{
			Context = context;
			this.Value = value;
			Index = index;
		}
	}

	public readonly struct ProduceContext
	{
		public readonly uint Count;
		public readonly uint? MaxExpected;
		public readonly Type Type;
		public readonly object Target;

		public ProduceContext(uint count, uint? maxExpected, Type type, object target)
		{
			Count = count;
			MaxExpected = maxExpected;
			Type = type;
			this.Target = target;
		}
	}

	public readonly struct ProducedValue
	{
		public readonly object? Value;
		public readonly bool Success;
		public readonly float Weight;

		public ProducedValue(object? value, bool success, float weight = 1)
		{
			Value = value;
			Success = success;
			Weight = weight;
		}
	}
}
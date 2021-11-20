using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace Needle.Timeline.CustomClipTools.ToolModule.Implementations
{
	// public abstract class BaseMultiFieldModifier : CoreToolModule
	// {
	// 	[Range(0,1)]
	// 	public float Radius = 1;
	// 	
	// 	public BaseMultiFieldModifier() => AllowBinding = true;
	//
	// 	protected override IList<Type> SupportedTypes { get; }= new[] { typeof(object) };
	//
	// 	private bool _triedFindingFields = false;
	// 	private readonly List<FieldInfo> _fields = new List<FieldInfo>();
	// 	private readonly List<IInterpolatable> _interpolatables = new List<IInterpolatable>();
	// 	private readonly List<object> _averagedValues = new List<object>();
	// 	
	// 	private class Data
	// 	{
	// 		public readonly List<object> Values = new List<object>();
	// 		public readonly List<float> Weights = new List<float>();
	// 	}
	//
	// 	public override bool OnModify(InputData input, ref ToolData toolData)
	// 	{
	// 		if (input.Stage == InputEventStage.Begin || input.Stage == InputEventStage.End)
	// 		{
	// 			OnReset();
	// 		}
	// 		return base.OnModify(input, ref toolData);
	// 	}
	//
	// 	protected virtual void OnReset()
	// 	{
	// 		_triedFindingFields = false;
	// 		_fields.Clear();
	// 		_interpolatables.Clear();
	// 		_averagedValues.Clear();
	// 	}
	//
	// 	protected override bool AllowedButton(MouseButton button)
	// 	{
	// 		return base.AllowedButton(button) || button == MouseButton.RightMouse;
	// 	}
	//
	// 	protected override ToolInputResult OnModifyValue(InputData input, ref ModifyContext context, ref object value)
	// 	{
	// 		if (!IsAnyBindingEnabled()) return ToolInputResult.AbortFurtherProcessing;
	// 		var pos = ToolHelpers.TryGetPosition(context.Object, value);
	// 		if (pos == null) return ToolInputResult.AbortFurtherProcessing;
	// 		var screenDistance = input.GetRadiusDistanceScreenSpace(Radius, pos.Value);
	// 		if (screenDistance <= 1)
	// 		{
	// 			if (!_triedFindingFields && _fields.Count <= 0)
	// 			{
	// 				foreach (var field in value.GetType().EnumerateFields())
	// 				{
	// 					if (!IsEnabled(field)) continue;
	// 					if (TryGetInterpolatable(field.FieldType, out var i) && i != null)
	// 					{
	// 						_fields.Add(field);
	// 						_interpolatables.Add(i);
	// 						_averagedValues.Add(Activator.CreateInstance(field.FieldType));
	// 					}
	// 				}
	// 			}
	// 			if (_fields.Count <= 0) return ToolInputResult.AbortFurtherProcessing;
	// 			
	// 			var data = new Data();
	// 			context.AdditionalData = data;
	// 			foreach (var field in _fields)
	// 			{
	// 				var currentValue = field.GetValue(value);
	// 				data.Values.Add(currentValue);
	// 				data.Weights.Add(1 - screenDistance.Value);
	// 			}
	// 			return ToolInputResult.CaptureForFinalize;
	// 		}
	// 		return ToolInputResult.Failed;
	// 	}
	//
	// 	protected override ToolInputResult OnModifyCaptured(InputData input, List<CapturedModifyContext> captured)
	// 	{
	// 		var t = 1f / captured.Count;
	// 		foreach (var cap in captured)
	// 		{
	// 			if (cap.Context.AdditionalData is Data b)
	// 			{
	// 				for (var i = 0; i < b.Values.Count; i++)
	// 				{
	// 					// accumulate a averaged default value for each field
	// 					var inter = _interpolatables[i];
	// 					var average = _averagedValues[i];
	// 					var value = b.Values[i];
	// 					inter.Interpolate(ref average, average, value, t);
	// 					_averagedValues[i] = average;
	// 				}
	// 			}
	// 			else return ToolInputResult.AbortFurtherProcessing;
	// 		}
	//
	// 		
	// 		t = .1f * Weight;
	// 		if (input.Button == MouseButton.RightMouse)
	// 			t *= -.3f;
	// 		foreach (var cap in captured)
	// 		{
	// 			if (cap.Context.AdditionalData is Data b)
	// 			{
	// 				for (var i = 0; i < b.Values.Count; i++)
	// 				{
	// 					// apply accumulated average per field
	// 					var inter = _interpolatables[i];
	// 					var val = b.Values[i];
	// 					inter.Interpolate(ref val, val, _averagedValues[i], t);
	// 					var field = _fields[i];
	// 					field.SetValue(cap.Value, val);
	// 				}
	// 			}
	// 		}
	// 		return ToolInputResult.Success;
	// 	}
	//
	// 	protected virtual void OnApply(int index, CapturedModifyContext context, int valueIndex, object value)
	// 	{
	// 		
	// 	}
	// }

	public class BlurModule : CoreToolModule, IWeighted
	{
		public float Radius = 1;

		[Range(0, 1)] public float Weight = 1;

		float IWeighted.Weight
		{
			get => Weight;
			set => Weight = value;
		}

		public BlurModule() => AllowBinding = true;

		protected override IList<Type> SupportedTypes { get; } = new[] { typeof(object) };

		private bool _triedFindingFields = false;
		private readonly List<FieldInfo> _fields = new List<FieldInfo>();
		private readonly List<IInterpolatable> _interpolatables = new List<IInterpolatable>();
		private readonly List<object> _averagedValues = new List<object>();

		private class BlurData
		{
			public readonly List<object> Values = new List<object>();
		}

		public override bool OnModify(InputData input, ref ToolData toolData)
		{
			if (input.Stage == InputEventStage.Begin || input.Stage == InputEventStage.End)
			{
				_triedFindingFields = false;
				_fields.Clear();
				_interpolatables.Clear();
				_averagedValues.Clear();
			}
			return base.OnModify(input, ref toolData);
		}

		protected override bool AllowedButton(MouseButton button)
		{
			return base.AllowedButton(button) || button == MouseButton.RightMouse;
		}

		protected override ToolInputResult OnModifyValue(InputData input, ref ModifyContext context, ref object value)
		{
			if (!IsAnyBindingEnabled()) return ToolInputResult.AbortFurtherProcessing;
			var pos = ToolHelpers.TryGetPosition(context.Object, value);
			if (pos == null) return ToolInputResult.AbortFurtherProcessing;
			var screenDistance = input.GetRadiusDistanceScreenSpace(Radius, pos.Value);
			if (screenDistance <= 1)
			{
				if (!_triedFindingFields && _fields.Count <= 0)
				{
					foreach (var field in value.GetType().EnumerateFields())
					{
						if (!IsEnabled(field)) continue;
						if (TryGetInterpolatable(field.FieldType, out var i) && i != null)
						{
							_fields.Add(field);
							_interpolatables.Add(i);
							_averagedValues.Add(Activator.CreateInstance(field.FieldType));
						}
					}
				}
				if (_fields.Count > 0)
				{
					var data = new BlurData();
					context.AdditionalData = data;
					foreach (var field in _fields)
					{
						var currentValue = field.GetValue(value);
						data.Values.Add(currentValue);
					}
					return ToolInputResult.CaptureForFinalize;
				}
			}
			return ToolInputResult.Failed;
		}

		protected override ToolInputResult OnModifyCaptured(InputData input, List<CapturedModifyContext> captured)
		{
			var t = 1f / captured.Count;
			foreach (var cap in captured)
			{
				if (cap.Context.AdditionalData is BlurData b)
				{
					for (var i = 0; i < b.Values.Count; i++)
					{
						// accumulate a averaged default value for each field
						var inter = _interpolatables[i];
						var average = _averagedValues[i];
						var value = b.Values[i];
						inter.Interpolate(ref average, average, value, t);
						_averagedValues[i] = average;
					}
				}
				else return ToolInputResult.AbortFurtherProcessing;
			}


			t = .1f * Weight;
			if (input.Button == MouseButton.RightMouse)
				t *= -.3f;
			foreach (var cap in captured)
			{
				if (cap.Context.AdditionalData is BlurData b)
				{
					for (var i = 0; i < b.Values.Count; i++)
					{
						// apply accumulated average per field
						var inter = _interpolatables[i];
						var val = b.Values[i];
						inter.Interpolate(ref val, val, _averagedValues[i], t);
						var field = _fields[i];
						field.SetValue(cap.Value, val);
					}
				}
			}
			return ToolInputResult.Success;
		}

		public class Picker : CoreToolModule, IWeighted
		{
			[Range(0, 1)] public float Radius = 1;

			[Range(0, 1)] public float Weight = 1;

			float IWeighted.Weight
			{
				get => Weight;
				set => Weight = value;
			}

			public Picker() => AllowBinding = true;

			protected override IList<Type> SupportedTypes { get; } = new[] { typeof(object) };

			private bool _triedFindingFields = false;
			private readonly List<FieldInfo> _fields = new List<FieldInfo>();
			private readonly List<IInterpolatable> _interpolatables = new List<IInterpolatable>();
			private readonly List<object> _averagedValues = new List<object>();

			private class Data
			{
				public readonly List<object> Values = new List<object>();
			}

			public override bool OnModify(InputData input, ref ToolData toolData)
			{
				if (input.Stage == InputEventStage.Begin || input.Stage == InputEventStage.End)
				{
					_triedFindingFields = false;
					_fields.Clear();
					_interpolatables.Clear();
					_averagedValues.Clear();
				}
				return base.OnModify(input, ref toolData);
			}

			protected override bool AllowedButton(MouseButton button)
			{
				return base.AllowedButton(button) || button == MouseButton.RightMouse;
			}

			protected override ToolInputResult OnModifyValue(InputData input, ref ModifyContext context, ref object value)
			{
				if (!IsAnyBindingEnabled()) return ToolInputResult.AbortFurtherProcessing;
				var pos = ToolHelpers.TryGetPosition(context.Object, value);
				if (pos == null) return ToolInputResult.AbortFurtherProcessing;
				var screenDistance = input.GetRadiusDistanceScreenSpace(Radius, pos.Value);
				if (screenDistance <= 1)
				{
					if (!_triedFindingFields && _fields.Count <= 0)
					{
						foreach (var field in value.GetType().EnumerateFields())
						{
							if (!IsEnabled(field)) 
								continue;
							if (TryGetInterpolatable(field.FieldType, out var i) && i != null)
							{
								_fields.Add(field);
								_interpolatables.Add(i);
								_averagedValues.Add(Activator.CreateInstance(field.FieldType));
							}
						}
					}
					if (_fields.Count > 0)
					{
						var data = new Data();
						context.AdditionalData = data;
						foreach (var field in _fields)
						{
							var currentValue = field.GetValue(value);
							data.Values.Add(currentValue);
						}
						return ToolInputResult.CaptureForFinalize;
					}
				}
				return ToolInputResult.Failed;
			}

			protected override ToolInputResult OnModifyCaptured(InputData input, List<CapturedModifyContext> captured)
			{
				var t = 1f / captured.Count;
				foreach (var cap in captured)
				{
					if (cap.Context.AdditionalData is Data b)
					{
						for (var i = 0; i < b.Values.Count; i++)
						{
							// accumulate a averaged default value for each field
							var inter = _interpolatables[i];
							var average = _averagedValues[i];
							var value = b.Values[i];
							inter.Interpolate(ref average, average, value, t);
							_averagedValues[i] = average;
						}
					}
					else return ToolInputResult.AbortFurtherProcessing;
				}


				t = .1f * Weight;
				if (input.Button == MouseButton.RightMouse)
					t *= -.3f;

				IBindsFields binder = this;
				for (var index = 0; index < _fields.Count; index++)
				{
					var field = _fields[index];
					var binding = binder.Bindings.FirstOrDefault(m => m.Matches(field));
					if (binding == null) throw new Exception("Expected to have binding for " + field);
					var value = _averagedValues[index];
					binding.ViewValue.SetValue(value);
				}
				return ToolInputResult.Success;
			}
		}
	}
}
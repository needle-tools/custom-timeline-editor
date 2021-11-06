using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Needle.Timeline
{
	public struct AnimationCurveWrapper : ICustomClip<float>
	{
		private readonly Func<AnimationCurve> curve;
		
		public string Name { get; set; }
		
		public AnimationCurveWrapper(Func<AnimationCurve> curve, string name)
		{
			this.curve = curve;
			Name = name;
			Changed = default;
			Keyframes = null;
			SupportedTypes = new[] { typeof(float) };
			Instance = null;
			ViewModel = null;
			IsRecording = false;
			RecordingStateChanged = default;
			#if UNITY_EDITOR
			AnimationUtility.onCurveWasModified += OnModify;
			#endif
		}

		#if UNITY_EDITOR
		private void OnModify(AnimationClip clip, EditorCurveBinding binding, AnimationUtility.CurveModifiedType type)
		{
			// TODO: use to update keyframes?!
		}
		#endif


		public float Evaluate(float time)
		{
			var res = curve().Evaluate(time);
			// Debug.Log(m_Name + " has " + res + " at " + time);
			return res;
		}

		public bool CanAdd(Type type)
		{
			return typeof(float).IsAssignableFrom(type);
		}

		public bool CanAdd(ICustomKeyframe kf)
		{
			return kf.value?.GetType() == typeof(float);
		}

		public bool Add(ICustomKeyframe kf)
		{
			return false;
		}

		public void Remove(ICustomKeyframe kf)
		{
			
		}

		#pragma warning disable CS0414
		public event Action Changed;
		#pragma warning restore CS0414
		
		public IReadOnlyCollection<IReadonlyCustomKeyframe> Keyframes { get; }
		
		public ICustomKeyframe GetPrevious(float time)
		{
			return null;
		}

		public ICustomKeyframe GetClosest(float time)
		{
			return null;
		}

		public Type[] SupportedTypes { get; private set; }
		
		[JsonIgnore] 
		public ClipInfoViewModel ViewModel { get; set; }

		object ICustomClip.Evaluate(float time)
		{
			return Evaluate(time);
		}

		public object Instance { get; set; }

		public bool CanInterpolate(Type type)
		{
			return typeof(float) == type;
		}

		public object Interpolate(object v0, object v1, float t)
		{
			var float0 = (float)v0;
			var float1 = (float)v1;
			return Mathf.Lerp(float0, float1, t);
		}

		public bool IsRecording { get; set; }
		public event Action RecordingStateChanged;
	}
}
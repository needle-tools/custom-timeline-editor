﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Needle.Timeline.Serialization;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Needle.Timeline
{
	public static class AnimationCurveBuilder
	{
		public readonly struct Data
		{
			public readonly CodeControlTrack Track;
			public readonly PlayableDirector Director;
			public readonly ClipInfoViewModel ViewModel;
			public readonly Type Type;
			public readonly EditorCurveBinding[] Bindings;
			public readonly TimelineClip TimelineClip;
			public readonly string Path;
			public readonly MemberInfo Member;
			public readonly Type MemberType;

			public Data(CodeControlTrack track,
				PlayableDirector director,
				ClipInfoViewModel viewModel,
				Type type,
				EditorCurveBinding[] bindings,
				TimelineClip timelineClip,
				string path,
				MemberInfo member,
				Type memberType)
			{
				Track = track;
				Director = director;
				ViewModel = viewModel;
				Type = type;
				Bindings = bindings;
				TimelineClip = timelineClip;
				Path = path;
				Member = member;
				MemberType = memberType;
			}
		}

		public enum CreationResult
		{
			None = 0,
			Successful = 1,
			NotMarked = 2,
			Failed = 3,
		}

		private static ProfilerMarker CreateMarker = new ProfilerMarker("CurveBuilder Create");

		public static CreationResult Create(Data data)
		{
			using (CreateMarker.Auto())
			{
				var attribute = data.Member.GetCustomAttribute<AnimateAttribute>();
				var res = CreateAnimationCurve(attribute, data);
				if (res == CreationResult.Successful) return res;

				res = CreateCustomAnimationCurve(attribute, data);

				return res;
			}
		}

		private static CreationResult CreateCustomAnimationCurve([CanBeNull] AnimateAttribute attribute, Data data)
		{
			if (attribute == null) return CreationResult.NotMarked;

			ICustomClip curve = default;

			if (Interpolators.TryFindInterpolator(attribute, data.MemberType, out var interpolator))
			{
				
			}
			
			if (data.MemberType == typeof(string))
			{
				// Debug.Log("Create string");
				curve = new CustomAnimationCurve<string>(new StringInterpolator(),
					new List<ICustomKeyframe<string>>()
					{
						new CustomKeyframe<string>("Hello", 0),
						new CustomKeyframe<string>("World", 1),
						new CustomKeyframe<string>("This is a very long string", 10)
					});
			}
			else if (data.MemberType == typeof(List<Vector3>))
			{
				var ser = new JsonSerializer();
				var content = SaveUtil.Load("test");
				if (content != null)
				{
					var type = typeof(CustomAnimationCurve<>).MakeGenericType(data.MemberType);
					curve = ser.Deserialize(content, type) as ICustomClip;
					if (curve is IHasInterpolator i) i.Interpolator = new ListInterpolator();
				}

				if (curve == null)
				{
					curve = new CustomAnimationCurve<List<Vector3>>(new ListInterpolator(),
						new List<ICustomKeyframe<List<Vector3>>>()
						{
							new CustomKeyframe<List<Vector3>>(
								new List<Vector3>() { Vector3.zero },
								(float)0
							),
							new CustomKeyframe<List<Vector3>>(
								GetPointsList(100),
								(float)5
							)
						});

					var json = (string)ser.Serialize(curve);
					SaveUtil.Save("test", json);
					json = SaveUtil.Load("test");
					Debug.Log("Loaded: " + json);
					curve = ser.Deserialize<CustomAnimationCurve<List<Vector3>>>(json);
				}
			}
			else if (data.Member == typeof(ComputeBuffer))
			{
				curve = new CustomAnimationCurve<ComputeBuffer>(new ComputeBufferInterpolator(typeof(float)));
			}
			else return CreationResult.Failed;

			object Resolve() => data.Director.GetGenericBinding(data.Track);
			var handler = new MemberWrapper(data.Member, Resolve, data.MemberType);
			data.ViewModel.Register(handler, curve);
			return CreationResult.Successful;
		}


		private static readonly Type[] animationCurveTypes =
		{
			typeof(float),
			typeof(int),
			typeof(double),
			typeof(uint),
		};

		private static CreationResult CreateAnimationCurve([CanBeNull] AnimateAttribute attribute, Data data)
		{
			if (!animationCurveTypes.Contains(data.MemberType))
			{
				if (attribute == null) return CreationResult.NotMarked;
				return CreationResult.Failed;
			}

			var binding = data.Bindings.FirstOrDefault(b => b.propertyName == data.Member.Name);
			// if the attribute has been removed
			if (attribute == null)
			{
				if (binding.propertyName != null)
				{
					Debug.Log("Remove curve: " + binding.propertyName);
					AnimationUtility.SetEditorCurve(data.TimelineClip.curves, binding, null);
				}

				return CreationResult.NotMarked;
			}

			// if the binding does not exist yet
			if (binding.propertyName == null)
			{
				Debug.Log("Create curve: " + data.Member.Name);
				var curve = new AnimationCurve();
				data.ViewModel.AnimationClip.SetCurve(data.Path, data.Type, data.Member.Name, curve);
				binding.propertyName = data.Member.Name;
				binding.type = data.Type;
				binding.path = data.Path;
			}

			object Resolve() => data.Director.GetGenericBinding(data.Track);
			var handler = new MemberWrapper(data.Member, Resolve, data.MemberType);
			var clip = data.TimelineClip.curves;
			var animationCurve = new AnimationCurveWrapper(() => AnimationUtility.GetEditorCurve(clip, binding), data.Member.Name);
			data.ViewModel.Register(handler, animationCurve);
			return CreationResult.Successful;
		}

		private static List<Vector3> GetPointsList(int count)
		{
			var list = new List<Vector3>();
			for (var i = 0; i < count; i++)
			{
				list.Add(UnityEngine.Random.insideUnitSphere);
			}

			return list;
		}
	}
}
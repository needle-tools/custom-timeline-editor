using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Needle.Timeline.Utils
{
	public static class AnimationCurveBuilder
	{
		public readonly struct Data
		{
			public readonly CodeTrack Track;
			public readonly PlayableDirector Director;
			public readonly ClipInfoViewModel ViewModel;
			public readonly Type Type;
			public readonly EditorCurveBinding[] Bindings;
			public readonly TimelineClip TimelineClip;
			public readonly string Path;
			public readonly MemberInfo Member;
			public readonly Type MemberType;

			public Data(CodeTrack track,
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


		private static readonly Type[] animationCurveTypes = new[]
		{
			typeof(float),
			typeof(int),
			typeof(double),
			typeof(uint),
		};

		public static bool Create(Data data)
		{
			if (CreateAnimationCurve(data)) return true;
			return false;
		}

		private static bool CreateAnimationCurve(Data data)
		{
			if (!animationCurveTypes.Contains(data.MemberType)) return false;

			var binding = data.Bindings.FirstOrDefault(b => b.propertyName == data.Member.Name);

			// if the attribute has been removed
			if (data.Member.GetCustomAttribute<AnimateAttribute>() == null)
			{
				if (binding.propertyName != null)
				{
					Debug.Log("Remove curve: " + binding.propertyName);
					AnimationUtility.SetEditorCurve(data.TimelineClip.curves, binding, null);
				}

				return false;
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
			return true;
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using UnityEngine.Timeline;

namespace Needle.Timeline
{
	public static class AnimationCurveBuilder
	{
		public class Data
		{
			public string Id => Member.Name;
			public readonly CodeControlTrack Track;
			public readonly CodeControlAsset TrackAsset;
			public readonly PlayableDirector Director;
			public readonly ClipInfoViewModel ViewModel;
			public readonly Type Type;
			public readonly TimelineClip TimelineClip;
			public readonly PlayableAsset Asset;

			public MemberInfo Member;
			public Type MemberType;
			public string TransformPath;

#if UNITY_EDITOR
			public EditorCurveBinding[] Bindings;
#endif

			public IEnumerable<string> EnumerateFormerNames(bool id)
			{
				var formerly = Member.GetCustomAttributes<FormerlySerializedAsAttribute>();
				var str = default(StringBuilder);
				foreach (var form in formerly)
				{
					if (!id) yield return form.oldName;
					else
					{
						str ??= new StringBuilder();
						str.Clear();
						str.Append(ViewModel.Id).Append("_").Append(form.oldName);
						yield return str.ToString();
					}
				}
			}

			public Data(
				CodeControlTrack track,
				CodeControlAsset trackAsset,
				PlayableDirector director,
				ClipInfoViewModel viewModel,
				Type type,
				TimelineClip timelineClip,
				PlayableAsset asset
			)
			{
				Track = track;
				this.TrackAsset = trackAsset;
				Director = director;
				ViewModel = viewModel;
				Type = type;
				TimelineClip = timelineClip;
				this.Asset = asset;
			}
		}

		public static string ToId(this ClipInfoViewModel vm, ICustomClip clip) => clip.Name;
		public static string ToId(this ClipInfoViewModel vm, MemberInfo member) => member.Name;

		public static ICustomClip Clone(ClipInfoViewModel viewModel, ICustomClip source)
		{
			var clip = CloneUtil.TryClone(source);
			clip.Id = viewModel.ToId(clip);
			if (clip is IHasInterpolator interpolatorTarget && source is IHasInterpolator interpolatorSource)
			{
				if (InterpolatorBuilder.TryFindInterpolator(
					    clip.SupportedTypes.First(),
					    out var interpolator,
					    interpolatorSource.Interpolator.GetType()))
					interpolatorTarget.Interpolator = interpolator;
				else  
					interpolatorTarget.Interpolator = new NoInterpolator();
			}
			return clip; 
		}
		
		public enum CreationResult
		{
			None = 0,
			Successful = 1,
			NotMarked = 2,
			Failed = 3,
		}

		public readonly struct Context
		{
			public readonly ILoader Loader;

			public Context(ILoader loader)
			{
				Loader = loader;
			}
		}

		private static ProfilerMarker CreateMarker = new ProfilerMarker("CurveBuilder Create");

		public static CreationResult Create(Data data, Context context)
		{
			using (CreateMarker.Auto())
			{
				var attribute = data.Member.GetCustomAttribute<AnimateAttribute>();
				var res = CreateAnimationCurve(attribute, data);
				if (res == CreationResult.Successful) return res;

				res = CreateCustomAnimationCurve(attribute, data, context);
				return res;
			}
		}

		private static CreationResult CreateCustomAnimationCurve([CanBeNull] AnimateAttribute attribute, Data data, Context context, out ICustomClip clip)
		{ 
			clip = default;
			if (attribute == null) return CreationResult.NotMarked;
			
			var curveType = typeof(CustomAnimationCurve<>).MakeGenericType(data.MemberType);
			try
			{
				var loader = context.Loader;
				if (loader == null) throw new Exception("CurveBuilderContext has no loader");
				var serContext = new SerializationContext(data.TimelineClip, data.TrackAsset);
				serContext.DisplayName = data.Member.Name;
				serContext.Type = curveType;
				var successfullyLoaded = loader.Load(data.Id, serContext, out var result);
				if (!successfullyLoaded)
				{
					foreach (var former in data.EnumerateFormerNames(true))
					{
						successfullyLoaded = loader.Load(former, serContext, out result);
						if (successfullyLoaded)
						{
							Debug.Log("<b>FOUND FORMERLY SERIALIZED</b>: " + former + " is now " + data.Member.Name);
							if (!loader.Rename(former, data.Id, serContext))
							{
								Debug.LogError("Failed updating former name for " + data.Member.Name + ", is this Id already assigned?");
							}
							break;
						}
					}
				}
				else if (!(result is ICustomClip)) throw new Exception("Loading succeeded but result is not a custom clip");

				clip = result as ICustomClip;
			}
			catch (Exception e)
			{
				data.ViewModel.failedLoading = true;
				Debug.LogException(e);
				Debug.LogError(data.Member + ", " + data.MemberType);
			}

			if (clip == null)
			{
#if !UNITY_EDITOR
				Debug.Log("Create new custom clip instance: " + curveType);
#endif
				clip = Activator.CreateInstance(curveType) as ICustomClip;
			}

			if (clip == null)
				return CreationResult.Failed;


			if (clip is IHasInterpolator i)
			{
				if (attribute.AllowInterpolation && InterpolatorBuilder.TryFindInterpolator(
					    data.MemberType, out var interpolator, attribute.Interpolator))
				{
					// Debug.Log("Chose " + interpolator + ", " + data.Member.Name);
					i.Interpolator = interpolator;
				}
				else
				{
					// Debug.Log("No Interpolator: " + data.Member.Name);
					i.Interpolator = new NoInterpolator();
				}
			}

			clip.Id = data.Id;
			clip.DisplayName = data.Member.Name;
			clip.Name = data.Member.Name;

			object Resolve() => data.ViewModel.Script; //data.Director.GetGenericBinding(data.Track);
			var handler = new MemberWrapper(data.Member, Resolve, data.MemberType);
			data.ViewModel.Register(handler, clip);
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
#if UNITY_EDITOR
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

			if (binding.propertyName == null)
			{
				foreach (var former in data.EnumerateFormerNames(false))
				{
					binding = data.Bindings.FirstOrDefault(b => b.propertyName == former);
					if (binding.propertyName != null)
					{
						// handle rename
						var curve = AnimationUtility.GetEditorCurve(data.TimelineClip.curves, binding);
						AnimationUtility.SetEditorCurve(data.TimelineClip.curves, binding, null);
						binding.propertyName = data.Member.Name;
						AnimationUtility.SetEditorCurve(data.TimelineClip.curves, binding, curve);
						Debug.Log("FOUND: " + data.Member.Name + " as " + former);
						break;
					}
				}
			}

			// if the binding does not exist yet
			if (binding.propertyName == null)
			{
				Debug.Log("Create curve: " + data.Member.Name);
				var curve = new AnimationCurve();
				data.ViewModel.AnimationClip.SetCurve(data.TransformPath, data.Type, data.Member.Name, curve);
				binding.propertyName = data.Member.Name;
				binding.type = data.Type;
				binding.path = data.TransformPath;
			}

			// Debug.Log(data.Member.Name + " on <b>" + data.ViewModel.Script?.GetType() + "</b>");
			object Resolve() => data.ViewModel.Script; // data.Director.GetGenericBinding(data.Track);
			var handler = new MemberWrapper(data.Member, Resolve, data.MemberType);
			var clip = data.TimelineClip.curves;
			var animationCurve = new AnimationCurveWrapper(() => AnimationUtility.GetEditorCurve(clip, binding), data.Member.Name);
			data.ViewModel.Register(handler, animationCurve);
			return CreationResult.Successful;
#else
			Debug.LogError("Not implemented");
			return CreationResult.Failed;
#endif
		}
	}
}
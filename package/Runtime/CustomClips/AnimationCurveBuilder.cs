using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Needle.Timeline.Serialization;
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
		public readonly struct Data
		{
			public readonly string Id;
			public readonly CodeControlTrack Track;
			public readonly PlayableDirector Director;
			public readonly ClipInfoViewModel ViewModel;
			public readonly Type Type;
			public readonly EditorCurveBinding[] Bindings;
			public readonly TimelineClip TimelineClip;
			public readonly string Path;
			public readonly MemberInfo Member;
			public readonly Type MemberType;
			public readonly PlayableAsset Asset;

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
				PlayableDirector director,
				ClipInfoViewModel viewModel,
				Type type,
				EditorCurveBinding[] bindings,
				TimelineClip timelineClip,
				string path,
				MemberInfo member,
				Type memberType,
				PlayableAsset asset
			)
			{
				this.Id = viewModel.Id + "_" + member.Name;
				Track = track;
				Director = director;
				ViewModel = viewModel;
				Type = type;
				Bindings = bindings;
				TimelineClip = timelineClip;
				Path = path;
				Member = member;
				MemberType = memberType;
				this.Asset = asset;
			}
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

				res = CreateCustomAnimationCurve(attribute, data, context, out var clip);
				if (clip != null) 
				{
					void OnClipOnChanged()
					{
						if (!data.Track)
						{
							clip.Changed -= OnClipOnChanged; 
							return;
						}
						// Debug.Log("clip changed");
						EditorUtility.SetDirty(data.Track);
						// TODO: figure out if we really need this
						data.Track.dirtyCount = (data.Track.dirtyCount + 1) % uint.MaxValue;
						data.Director.Evaluate();
						TimelineWindowUtil.TryRepaint();
					}

					clip.Changed += OnClipOnChanged;
				}
				return res;
			}
		}

		private static CreationResult CreateCustomAnimationCurve([CanBeNull] AnimateAttribute attribute, Data data, Context context, out ICustomClip curve)
		{
			curve = default;
			if (attribute == null) return CreationResult.NotMarked;

			var name = data.Member.Name;//.ToLowerInvariant();
			// var ser = new JsonSerializer();

			var curveType = typeof(CustomAnimationCurve<>).MakeGenericType(data.MemberType);
			try
			{
				var loader = context.Loader;
				var serContext = new SerializationContext(data.TimelineClip);
				serContext.Type = curveType;
				var successfullyLoaded = loader.Load(data.Id, serContext, out var result);
				if (!successfullyLoaded)
				{
					foreach (var former in data.EnumerateFormerNames(true))
					{
						successfullyLoaded = loader.Load(former, serContext, out result);
						if (successfullyLoaded)
						{
							Debug.Log("<b>FOUND FORMERLY SERIALIZED</b>: " + former + " is now "  + data.Member.Name);
							if (!loader.Rename(former, data.Id, serContext))
							{
								Debug.LogError("Failed updating former name for " + data.Member.Name + ", is this Id already assigned?");
							}
							break;
						}
					}
				}
				else if(!(result is ICustomClip)) throw new Exception("Loading succeeded but result is not a custom clip");

				curve = result as ICustomClip;
			}
			catch (Exception e)
			{
				Debug.LogError(data.Member + ", " + data.MemberType);
				Debug.LogException(e);
			}

			if (curve == null)
			{
				curve = Activator.CreateInstance(curveType) as ICustomClip;
			}

			if (curve == null)
				return CreationResult.Failed;

			
			if (curve is IHasInterpolator i)
			{
				if (InterpolatorBuilder.TryFindInterpolator(attribute, data.MemberType, out var interpolator))
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

			curve.Id = data.Id;
			curve.Name = name;

			object Resolve() => data.ViewModel.Script; //data.Director.GetGenericBinding(data.Track);
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
				data.ViewModel.AnimationClip.SetCurve(data.Path, data.Type, data.Member.Name, curve);
				binding.propertyName = data.Member.Name;
				binding.type = data.Type;
				binding.path = data.Path;
			}

			// Debug.Log(data.Member.Name + " on <b>" + data.ViewModel.Script?.GetType() + "</b>");
			object Resolve() => data.ViewModel.Script; // data.Director.GetGenericBinding(data.Track);
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
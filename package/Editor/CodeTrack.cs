using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DefaultNamespace;
using Editor;
using Unity.Jobs;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Needle.Timeline
{
	[Serializable]
	public class ClipInfoModel
	{
		public string id;
		public AnimationClip clip;
	}

	public class ClipInfoViewModel
	{
		private ClipInfoModel model;

		public ClipInfoViewModel(ClipInfoModel model)
		{
			this.model = model;
		}

		internal void Register(IValueHandler handler, ICustomClip clip)
		{
			values.Add(handler);
			clips.Add(clip);
		}

		internal AnimationClip AnimationClip => model.clip;

		public List<IValueHandler> values = new List<IValueHandler>();
		public List<ICustomClip> clips = new List<ICustomClip>();
	}

	[TrackClipType(typeof(CodeControlAsset))]
	[TrackBindingType(typeof(MonoBehaviour))]
	[TrackColor(.2f, .5f, 1f)]
	public class CodeTrack : TrackAsset
	{
		[SerializeField] private List<ClipInfoModel> clips = new List<ClipInfoModel>();

		[NonSerialized] private List<ClipInfoViewModel> viewModels = new List<ClipInfoViewModel>();


		// public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
		// {
		// 	return ScriptPlayable<CodeControlTrackMixer>.Create(graph, inputCount);
		// }

		private static BindingFlags DefaultFlags => BindingFlags.Default | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

		protected override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip timelineClip)
		{
			var dir = gameObject.GetComponent<PlayableDirector>();
			var boundObject = dir.GetGenericBinding(this) as MonoBehaviour;
			if (!boundObject) return Playable.Null;

			var asset = timelineClip.asset as CodeControlAsset;
			if (!asset) throw new NullReferenceException("Missing code control asset");

			var animationComponents = boundObject.GetComponents<IAnimated>();
			if (animationComponents.Length <= 0) return Playable.Null;

			Debug.Log("<b>Create Playable</b> " + graph, timelineClip.asset);
			timelineClip.CreateCurves("Procedural");

			if (!ObjectIdentifier.TryGetObjectIdentifier(asset, out var objectIdentifier))
				throw new Exception("Failed getting object identifier for track asset: " + asset);

			var id = objectIdentifier.guid + "@" + objectIdentifier.localIdentifierInFile;
			foreach (var anim in animationComponents)
			{
				var model = clips.FirstOrDefault(clipInfo => clipInfo.id == id);
				if (model == null) model = new ClipInfoModel();
				if (model.id == null)
				{
					model.id = id;
					model.clip = timelineClip.curves;
					timelineClip.displayName += "\n" + id;
					clips.Add(model);
				}
				
				var viewModel = new ClipInfoViewModel(model);
				asset.viewModel = viewModel;
				viewModels.Add(viewModel);

				var type = anim.GetType();
				var animationClip = timelineClip.curves;
				var clipBindings = AnimationUtility.GetCurveBindings(animationClip);
				var path = AnimationUtility.CalculateTransformPath(boundObject.transform, null);
				
				var fields = type.GetFields(DefaultFlags);
				foreach (var field in fields)
				{
					Create(dir, viewModel, type, clipBindings, timelineClip, path, field, field.FieldType);
				}
				
				var properties = type.GetProperties(DefaultFlags);
				foreach (var prop in properties)
				{
					Create(dir, viewModel, type, clipBindings, timelineClip, path, prop, prop.PropertyType);
				}
			}

			// var curve = AnimationCurve.Linear(0, 0, 1, 1);
			var playable = base.CreatePlayable(graph, gameObject, timelineClip);
			return playable;
		}

		private void Create(PlayableDirector director,
			ClipInfoViewModel viewModel,
			Type type,
			EditorCurveBinding[] bindings,
			TimelineClip timelineClip,
			string path,
			MemberInfo member,
			Type memberType)
		{
			if (memberType != typeof(float)) return;

			var binding = bindings.FirstOrDefault(b => b.propertyName == member.Name);
			
			// if the attribute has been removed
			if (member.GetCustomAttribute<AnimateAttribute>() == null)
			{
				if (binding.propertyName != null)
				{
					Debug.Log("Remove curve: " + binding.propertyName);
					AnimationUtility.SetEditorCurve(timelineClip.curves, binding, null);
				}

				return;
			}

			// if the binding does not exist yet
			if (binding.propertyName == null)
			{
				Debug.Log("Create curve: " + member.Name);
				var curve = new AnimationCurve();
				viewModel.AnimationClip.SetCurve(path, type, member.Name, curve);
				binding.propertyName = member.Name;
				binding.type = type;
				binding.path = path;
			}
			
			object Resolve() => director.GetGenericBinding(this);
			var handler = new MemberWrapper(member, Resolve);
			var clip = timelineClip.curves;
			var animationCurve = new AnimationCurveWrapper(() => AnimationUtility.GetEditorCurve(clip, binding), member.Name);
			viewModel.Register(handler, animationCurve);
		}


		public readonly struct MemberWrapper : IValueHandler
		{
			private readonly MemberInfo member;
			private readonly Func<object> getTarget;

			public MemberWrapper(MemberInfo member, Func<object> getTarget)
			{
				this.member = member;
				this.getTarget = getTarget; 
			}

			public void SetValue(object value)
			{
				var target = getTarget();
				if (target == null)
				{
					// Debug.Log("Target is null, not setting " + value);
					return;
				}
				// Debug.Log("set " + value + " on " + target, target as UnityEngine.Object);
				switch (member)
				{
					case FieldInfo field:
						field.SetValue(target, value);
						break;
					case PropertyInfo property:
						if (property.CanWrite)
							property.SetValue(target, value);
						break;
				}
			}
		}
	}
}
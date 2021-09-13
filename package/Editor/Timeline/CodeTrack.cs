using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DefaultNamespace;
using Needle.Timeline.Utils;
using Unity.Jobs;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Needle.Timeline
{
	[TrackClipType(typeof(CodeControlAsset))]
	[TrackBindingType(typeof(MonoBehaviour))]
	[TrackColor(.2f, .5f, 1f)]
	public class CodeTrack : TrackAsset
	{
		[SerializeField] private List<ClipInfoModel> clips = new List<ClipInfoModel>();
		[NonSerialized] private readonly List<ClipInfoViewModel> viewModels = new List<ClipInfoViewModel>();
		internal static readonly bool IsUsingMixer = true;

		public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
		{
			if (IsUsingMixer)
				return ScriptPlayable<CodeControlTrackMixer>.Create(graph, inputCount);
			return Playable.Null;
		}

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
					var data = new AnimationCurveBuilder.Data(this, dir, viewModel, type, clipBindings, timelineClip, path, 
						field, field.FieldType);
					if(AnimationCurveBuilder.Create(data) == AnimationCurveBuilder.CreationResult.Failed)
						OnFailedCreatingCurves(field.FieldType);
				}

				var properties = type.GetProperties(DefaultFlags);
				foreach (var prop in properties)
				{
					var data = new AnimationCurveBuilder.Data(this, dir, viewModel, type, clipBindings, timelineClip, path,
						prop, prop.PropertyType);
					if(AnimationCurveBuilder.Create(data) == AnimationCurveBuilder.CreationResult.Failed)
						OnFailedCreatingCurves(prop.PropertyType);
				}
			}

			// var curve = AnimationCurve.Linear(0, 0, 1, 1);
			var playable = base.CreatePlayable(graph, gameObject, timelineClip);
			return playable;
		}

		private void OnFailedCreatingCurves(Type type)
		{
			Debug.LogWarning("<b>Failed creating curves for</b> " + type);
		}
	}
}
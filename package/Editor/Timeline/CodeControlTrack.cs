using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Profiling;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Needle.Timeline
{
	/*
	 *	TODO: where to serialize keyframe data?
	 *	TODO: real mixer interpolation
	 * 
	 */

	[TrackClipType(typeof(CodeControlAsset))]
	[TrackBindingType(typeof(MonoBehaviour))]
	[TrackColor(.2f, .5f, 1f)]
	public class CodeControlTrack : TrackAsset, ICanDrawInlineCurve
	{
		[SerializeField] private List<ClipInfoModel> clips = new List<ClipInfoModel>();
		
		

		protected override void OnBeforeTrackSerialize()
		{
			Debug.Log("TODO: save state");
			base.OnBeforeTrackSerialize();
		}

		protected override void OnAfterTrackDeserialize()
		{
			base.OnAfterTrackDeserialize();
		}
		
		
		[NonSerialized] private readonly List<ClipInfoViewModel> viewModels = new List<ClipInfoViewModel>();
		internal static readonly bool IsUsingMixer = true;

		public bool CanDraw() => true;

		public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
		{
			if (IsUsingMixer)
				return ScriptPlayable<CodeControlTrackMixer>.Create(graph, inputCount);
			return Playable.Null;
		}

		private static ProfilerMarker CreateTrackMarker = new ProfilerMarker("Create Playable Track");

		private static BindingFlags DefaultFlags => BindingFlags.Default | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

		protected override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip timelineClip)
		{
			using (CreateTrackMarker.Auto())
			{
				var dir = gameObject.GetComponent<PlayableDirector>();
				var boundObject = dir.GetGenericBinding(this) as MonoBehaviour;
				if (!boundObject) return Playable.Null;

				var asset = timelineClip.asset as CodeControlAsset;
				if (!asset) throw new NullReferenceException("Missing code control asset");
				
				Debug.Log("Create " + asset);

				var animationComponents = boundObject.GetComponents<IAnimated>();
				if (animationComponents.Length <= 0) return Playable.Null;

				// Debug.Log("<b>Create Playable</b> " + graph, timelineClip.asset);
				timelineClip.CreateCurves("Procedural");

				if (!ObjectIdentifier.TryGetObjectIdentifier(asset, out var objectIdentifier))
					throw new Exception("Failed getting object identifier for track asset: " + asset);

				var id = objectIdentifier.guid + "@" + objectIdentifier.localIdentifierInFile;
				foreach (var anim in animationComponents)
				{
					var model = clips.FirstOrDefault(clipInfo => clipInfo.id == id);
					if (model == null)
					{
						model = new ClipInfoModel(id, timelineClip.curves);
						// timelineClip.displayName += "\n" + id;
						clips.Add(model);
					}

					timelineClip.displayName = "Code Track";

					var viewModel = new ClipInfoViewModel(model);
					viewModel.startTime = timelineClip.start;
					viewModel.endTime = timelineClip.end;
					viewModel.timeScale = timelineClip.timeScale;
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
						if (AnimationCurveBuilder.Create(data) == AnimationCurveBuilder.CreationResult.Failed)
							OnFailedCreatingCurves(field.FieldType);
					}

					var properties = type.GetProperties(DefaultFlags);
					foreach (var prop in properties)
					{
						var data = new AnimationCurveBuilder.Data(this, dir, viewModel, type, clipBindings, timelineClip, path,
							prop, prop.PropertyType);
						if (AnimationCurveBuilder.Create(data) == AnimationCurveBuilder.CreationResult.Failed)
							OnFailedCreatingCurves(prop.PropertyType);
					}
				}

				// var curve = AnimationCurve.Linear(0, 0, 1, 1);
				var playable = base.CreatePlayable(graph, gameObject, timelineClip);
				return playable;
			}
		}

		private static void OnFailedCreatingCurves(Type type)
		{
			Debug.LogWarning("<b>Failed creating curves for</b> " + type);
		}
	}
}
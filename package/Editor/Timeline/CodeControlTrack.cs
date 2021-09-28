using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Needle.Timeline.Serialization;
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
		// protected override void OnBeforeTrackSerialize()
		// {
		// 	Debug.Log("TODO: save state " + EditorUtility.IsDirty(this)); 
		// 	base.OnBeforeTrackSerialize();
		// }
		//
		// protected override void OnAfterTrackDeserialize()
		// {
		// 	base.OnAfterTrackDeserialize();
		// }

		internal void Save()
		{
			var ser = new JsonSerializer();
			foreach (var viewModel in viewModels)
			{
				if (!viewModel.IsValid) continue;
				foreach (var clip in viewModel.clips)
				{
					var json = (string)ser.Serialize(clip);
					var id = viewModel.Id + "_" + clip.Name;
					SaveUtil.Save(id, json); 
					Debug.Log("saved " + id);
				}
			}
		}
		
		internal const bool IsUsingMixer = true;

		[SerializeField, HideInInspector] internal int dirtyCount;
		[SerializeField, HideInInspector] private List<ClipInfoModel> clips = new List<ClipInfoModel>();
		[NonSerialized] private readonly List<ClipInfoViewModel> viewModels = new List<ClipInfoViewModel>();

		internal IReadOnlyList<ClipInfoViewModel> ViewModels => viewModels;

		public bool CanDraw() => true;

		public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
		{
			return ScriptPlayable<CodeControlTrackMixer>.Create(graph, inputCount);
		}

		private static ProfilerMarker CreateTrackMarker = new ProfilerMarker("Create Playable Track");

		private static BindingFlags DefaultFlags => BindingFlags.Default | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

		protected override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip timelineClip)
		{
			using (CreateTrackMarker.Auto())
			{
				for (var i = viewModels.Count - 1; i >= 0; i--)
				{
					var vm = viewModels[i];
					if (!vm.IsValid) viewModels.RemoveAt(i);
				}
				
				var dir = gameObject.GetComponent<PlayableDirector>();
				var assetPath = AssetDatabase.GetAssetPath(dir.playableAsset);
				UnitySaveProcessor.Register(this, assetPath);
				
				var boundObject = dir.GetGenericBinding(this) as MonoBehaviour;
				if (!boundObject) return Playable.Null;

				var asset = timelineClip.asset as CodeControlAsset;
				if (!asset) throw new NullReferenceException("Missing code control asset");
				
				// Debug.Log("Create " + asset + ", " + viewModels.Count);

				var animationComponents = boundObject.GetComponents<IAnimated>();
				if (animationComponents.Length <= 0) return Playable.Null;
				
				
				AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset.GetInstanceID(), out var guid, out long _id);
				var id = guid + "@" + _id; 

				// Debug.Log("<b>Create Playable</b> " + boundObject, timelineClip.asset);
				timelineClip.CreateCurves(id);
				
				foreach (var script in animationComponents)
				{
					// Debug.Log(script);
					var model = clips.FirstOrDefault(clipInfo => clipInfo.id == id);
					if (model == null)
					{
						model = new ClipInfoModel(id, timelineClip.curves);
						// timelineClip.displayName += "\n" + id;
						clips.Add(model);
					}

					var existing = viewModels.FirstOrDefault(v => v.Script == script && v.AnimationClip == timelineClip.curves);
					timelineClip.displayName = script.GetType().Name;

					var viewModel = existing ?? new ClipInfoViewModel(boundObject.name, script, model);
					viewModel.director = dir;
					viewModel.startTime = timelineClip.start;
					viewModel.endTime = timelineClip.end;
					viewModel.length = viewModel.endTime - viewModel.startTime;
					viewModel.timeScale = timelineClip.timeScale;
					if(!asset.viewModels.Contains(viewModel))
						asset.viewModels.Add(viewModel);
					if (existing != null) continue;
					viewModels.Add(viewModel);

					var type = script.GetType();
					var animationClip = timelineClip.curves;
					var clipBindings = AnimationUtility.GetCurveBindings(animationClip);
					var path = AnimationUtility.CalculateTransformPath(boundObject.transform, null);
					
					// TODO: handle formerly serialized name
					
					var fields = type.GetFields(DefaultFlags);
					foreach (var field in fields) 
					{
						var data = new AnimationCurveBuilder.Data(this, dir, viewModel, type, clipBindings, timelineClip, path,
							field, field.FieldType, dir.playableAsset);
						if (AnimationCurveBuilder.Create(data) == AnimationCurveBuilder.CreationResult.Failed)
							OnFailedCreatingCurves(field.FieldType);
					}

					var properties = type.GetProperties(DefaultFlags);
					foreach (var prop in properties)
					{
						var data = new AnimationCurveBuilder.Data(this, dir, viewModel, type, clipBindings, timelineClip, path,
							prop, prop.PropertyType, dir.playableAsset);
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
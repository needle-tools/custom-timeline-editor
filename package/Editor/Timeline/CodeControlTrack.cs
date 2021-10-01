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
					// Debug.Log("saved " + id);
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

		/// <summary>
		/// For id generation per gameobject / type
		/// </summary>
		private readonly List<(string type, int index)> componentTypeIndices = new List<(string, int)>();
		
		protected override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip timelineClip)
		{
			using (CreateTrackMarker.Auto())
			{
				viewModels.RemoveAll(vm => !vm.IsValid);
				
				var dir = gameObject.GetComponent<PlayableDirector>();
				var assetPath = AssetDatabase.GetAssetPath(dir.playableAsset);
				UnitySaveUtil.Register(this, assetPath);
				
				var boundObject = dir.GetGenericBinding(this) as MonoBehaviour;
				if (!boundObject) return Playable.Null;

				var asset = timelineClip.asset as CodeControlAsset;
				if (!asset) throw new NullReferenceException("Missing code control asset");
				asset.name = gameObject.name;
				asset.viewModels?.RemoveAll(vm => !vm.IsValid);

				// Debug.Log("Create " + asset + ", " + viewModels.Count);

				var animationComponents = boundObject.GetComponents<IAnimated>();
				if (animationComponents.Length <= 0) return Playable.Null;
				
				
				AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset.GetInstanceID(), out var guid, out long _id);
				var id = guid + "@" + _id;
				
				// Debug.Log("<b>Create Playable</b> " + boundObject, timelineClip.asset);
				timelineClip.CreateCurves(id);
				
				componentTypeIndices.Clear();
				foreach (var script in animationComponents)
				{
					var type = script.GetType();
					var typeName = type.Name;
					var index = -1;
					for (var i = 0; i < componentTypeIndices.Count; i++)
					{
						var ct = componentTypeIndices[i];
						if (ct.type == typeName)
						{
							index = ++ct.index;
							componentTypeIndices[i] = (typeName, index);
						}
					}
					if (index < 0)
					{
						index = 0;
						componentTypeIndices.Add((typeName, 0));
					}
					var modelId = id + "_" + typeName + "_" + index;
					
					var model = clips.FirstOrDefault(e => e.id == modelId);
					if (model == null)
					{
						// Debug.Log("Create Clip Model: " + modelId);
						model = new ClipInfoModel(modelId, timelineClip.curves);
						clips.Add(model);
					}
					
					timelineClip.displayName = typeName;

					bool GetExisting(ClipInfoViewModel v) => v.Script == script && v.AnimationClip == timelineClip.curves && v.Id == model.id;
					var existing = viewModels.FirstOrDefault(GetExisting);
					if (existing == null && asset.viewModels != null)
						existing = asset.viewModels.FirstOrDefault(GetExisting);

					// Debug.Log("existing?? " + existing); 
					var viewModel = existing ?? new ClipInfoViewModel(boundObject.name, script, model, timelineClip);
					viewModel.director = dir;
					if (existing != null)
					{
						// Debug.Log("VM exists: " + id);
						continue;
					}
					// Debug.Log("Add VM");
					if (!asset.viewModels?.Contains(viewModel) ?? false)
						asset.viewModels.Add(viewModel);
					viewModels.Add(viewModel);

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
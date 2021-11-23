using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Needle.Timeline.Models;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Timeline;
#endif

namespace Needle.Timeline
{
	[TrackClipType(typeof(CodeControlAsset))]
	[TrackBindingType(typeof(MonoBehaviour))]
	[TrackColor(.2f, .5f, 1f)]
	public class CodeControlTrack : TrackAsset
#if UNITY_EDITOR
		, ICanDrawInlineCurve
#endif
	{
		private readonly List<PlayableDirector> directorsChanged = new List<PlayableDirector>();
		internal void Save()
		{
			// TODO: handle delete (remove serialized data)
			// TODO: how can we prevent override by accident -> e.g. field name Point and point

			var loader = LoadersRegistry.GetDefault();
			if (loader == null) throw new Exception("Failed getting default loader");
			foreach (var viewModel in viewModels)
			{
				if (!viewModel.IsValid) continue;
				if (!viewModel.HasUnsavedChanges) continue;
				if (viewModel.Save(loader))
				{
					if(!directorsChanged.Contains(viewModel.director))
						directorsChanged.Add(viewModel.director);
				}
				viewModel.HasUnsavedChanges = false;
			}
			TempFileLocationLoader.DeleteTempUnsavedChangesDirectory();
			if (directorsChanged.Count > 0)
			{
				Debug.Log("<b>SAVED</b>");
#if UNITY_EDITOR
				AssetDatabase.SaveAssets();
#endif
				foreach (var dir in directorsChanged)
				{ 
					if(dir)
						dir.RebuildGraph(); 
				}
			}
		}

		[SerializeField] internal TrackModel model = new TrackModel();
		[SerializeField, HideInInspector] internal uint dirtyCount;
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

#if UNITY_EDITOR
				var assetPath = AssetDatabase.GetAssetPath(dir.playableAsset);
				UnitySaveUtil.Register(this, assetPath);
#endif

				var boundObject = dir.GetGenericBinding(this) as MonoBehaviour;
				if (!boundObject) return Playable.Null;

				var asset = timelineClip.asset as CodeControlAsset;
				if (!asset) throw new NullReferenceException("Missing code control asset");
				asset.name = gameObject.name;
				asset.viewModels?.RemoveAll(vm => !vm.IsValid);

				var animationComponents = boundObject.GetComponents<IAnimated>();
				if (animationComponents.Length <= 0) return Playable.Null;

				var id = default(string);
#if UNITY_EDITOR
				// TODO: use something else than the asset as a identifier for saving/loading data - it would be cool to be able to use animated data in other contexts (other timeline, other TrackAsset instances, other script instances) as well
				if (asset && asset.data)
				{
					AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset.GetInstanceID(), out var guid, out long _id);
					id = guid + "@" + _id;
					asset.id = id;
				}
#else
				id = asset.id;
#endif

// #if UNITY_EDITOR
// 				Debug.Log("Create " + asset.data, asset);
// #endif

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
					var modelId = $"{id}_{typeName}_{index}";
 
					var model = clips.FirstOrDefault(e => e.id == modelId);
					if (model == null)
					{
						// Debug.Log("Create Clip Model: " + modelId);
						model = new ClipInfoModel(modelId, timelineClip.curves);
						clips.Add(model);
					}

					timelineClip.displayName = typeName;

					bool GetExisting(ClipInfoViewModel v)
					{
						var match = v.AnimationClip == timelineClip.curves && v.Id == model.id;
						if (match && (v.Script == script || v.Script == null || v.Script is Object o && !o))
							return true;
						return false;
					}

					var existing = viewModels.FirstOrDefault(GetExisting);
					if (existing == null && asset.viewModels != null)
						existing = asset.viewModels.FirstOrDefault(GetExisting);

					// Debug.Log("existing?? " + existing); 
					var viewModel = existing ?? new ClipInfoViewModel(this, boundObject.name, script, model, timelineClip);
					viewModel.director = dir;
					viewModel.asset = asset;
					viewModel.Script = script;
					if (existing != null)
					{
						if(!existing.RequiresReload)
							continue;
						viewModels.Remove(existing);
						existing.Clear();
					}
					// Debug.Log("Add VM");
					if (!asset.viewModels?.Contains(viewModel) ?? false)
						asset.viewModels.Add(viewModel);
					viewModels.Add(viewModel);
					viewModel.RequiresReload = false;


					var loader = new TempFileLocationLoader(LoadersRegistry.GetDefault());
					if (loader == null) throw new Exception("Missing default loader");
					var context = new AnimationCurveBuilder.Context(loader);

					var fields = type.GetFields(DefaultFlags);
					var data = new AnimationCurveBuilder.Data(this, asset, dir, viewModel, type, timelineClip, dir.playableAsset);

					// TODO: make animation curves work for runtime
#if UNITY_EDITOR
					var animationClip = timelineClip.curves;
					var clipBindings = AnimationUtility.GetCurveBindings(animationClip);
					var path = AnimationUtility.CalculateTransformPath(boundObject.transform, null);
					data.TransformPath = path;
					data.Bindings = clipBindings;
#endif

					foreach (var field in fields)
					{
						data.Member = field;
						data.MemberType = field.FieldType;
						if (AnimationCurveBuilder.Create(data, context) is AnimationCurveBuilder.CreationResult.Failed)
							OnFailedCreatingCurves(field.FieldType);
					}

					var properties = type.GetProperties(DefaultFlags);
					foreach (var prop in properties)
					{
						data.Member = prop;
						data.MemberType = prop.PropertyType;
						if (AnimationCurveBuilder.Create(data, context) is AnimationCurveBuilder.CreationResult.Failed)
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
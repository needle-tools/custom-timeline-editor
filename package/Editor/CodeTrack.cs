using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DefaultNamespace;
using Editor;
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

		[NonSerialized] public List<IValueHandler> values = new List<IValueHandler>();
		[NonSerialized] public List<ICustomClip> clips = new List<ICustomClip>();
	}

	[TrackClipType(typeof(CodeControlAsset))]
	[TrackBindingType(typeof(MonoBehaviour))]
	[TrackColor(.2f, .5f, 1f)]
	public class CodeTrack : TrackAsset
	{
		[SerializeField] private List<ClipInfoModel> clips = new List<ClipInfoModel>();


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

				var animationClip = timelineClip.curves;
				asset.model = model;

				if (model.values == null) 
					model.values = new List<IValueHandler>();
				model.values.Clear();
				if (model.clips == null) 
					model.clips = new List<ICustomClip>();
				model.clips.Clear();

				var type = anim.GetType();
				var clipBindings = AnimationUtility.GetCurveBindings(animationClip);
				var path = AnimationUtility.CalculateTransformPath(boundObject.transform, null);
				// Debug.Log(path);


				var fields = anim.GetType().GetFields(DefaultFlags);
				foreach (var field in fields)
				{
					if (field.FieldType != typeof(float)) continue;

					var binding = clipBindings.FirstOrDefault(b => b.propertyName == field.Name);
					if (field.GetCustomAttribute<AnimateAttribute>() == null)
					{
						if (binding.propertyName != null)
						{
							Debug.Log("Remove curve: " + binding.propertyName);
							AnimationUtility.SetEditorCurve(timelineClip.curves, binding, null);
						}

						continue;
					}

					AnimationCurve curve;
					if (binding.propertyName == null)
					{
						Debug.Log("Create curve: " + field.Name);
						curve = new AnimationCurve();
						model.clip.SetCurve(path, type, field.Name, curve);
					}
					else
					{
						curve = AnimationUtility.GetEditorCurve(timelineClip.curves, binding);
					}


					object Resolve() => dir.GetGenericBinding(this);
					var handler = new FloatField(field, Resolve);
					model.values.Add(handler);
					model.clips.Add(new AnimationCurveWrapper(curve));
				}
			}

			// var curve = AnimationCurve.Linear(0, 0, 1, 1);
			var playable = base.CreatePlayable(graph, gameObject, timelineClip);
			return playable;
		}

		public struct FloatField : IValueHandler
		{
			private readonly FieldInfo field;
			private readonly Func<object> getTarget;

			public FloatField(FieldInfo field, Func<object> getTarget)
			{
				this.field = field;
				this.getTarget = getTarget;
			}

			public void SetValue(object value)
			{
				var target = getTarget();
				if (target == null) return;
				field.SetValue(target, value);
			}
		}
	}
}
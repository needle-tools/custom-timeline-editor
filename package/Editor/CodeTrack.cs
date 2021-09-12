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
			
		[NonSerialized]
		public List<IValueHandler> values;
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

			Debug.Log("<b>Create Playable</b> " + graph, timelineClip.asset);
			
			timelineClip.CreateCurves("Procedural");
			
			if(!boundObject) return Playable.Null;


			var asset = timelineClip.asset as CodeControlAsset;
			if (!asset) throw new NullReferenceException("Missing code control asset");
			
			
			ObjectIdentifier.TryGetObjectIdentifier(asset, out var objectIdentifier);
			var id = objectIdentifier.guid + "@" + objectIdentifier.localIdentifierInFile;

			foreach (var anim in boundObject.GetComponents<IAnimated>())
			{
				var model = clips.FirstOrDefault(clipInfo => clipInfo.id == id);
				if (model == null) model = new ClipInfoModel();
				if (model.id == null)
				{
					// Debug.Log("CREATE " + id);
					model.id = id;
					model.clip = timelineClip.curves;
					timelineClip.displayName += "\n" + id;
					clips.Add(model); 
				}
				
				var animationClip = timelineClip.curves;
				asset.model = model;

				if (model.values == null) model.values = new List<IValueHandler>();
				model.values.Clear();

				var type = anim.GetType();
				var clipBindings = AnimationUtility.GetCurveBindings(animationClip);
				var path = AnimationUtility.CalculateTransformPath(boundObject.transform, null);
				// Debug.Log(path);

				
				var fields = anim.GetType().GetFields(DefaultFlags);
				foreach (var field in fields)
				{
					if (field.FieldType != typeof(float)) continue;

					object Resolve() => dir.GetGenericBinding(this);
					var handler = new FloatField(field, Resolve);
					model.values.Add(handler);
					

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

					if (binding.propertyName == null)
					{
						Debug.Log("Create curve: " + field.Name);
						model.clip.SetCurve(path, type, field.Name, new AnimationCurve());
					}
				}
			}

			// var curve = AnimationCurve.Linear(0, 0, 1, 1);
			var playable = base.CreatePlayable(graph, gameObject, timelineClip);
			return playable;
		}
		
		public class FloatField : IValueHandler
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
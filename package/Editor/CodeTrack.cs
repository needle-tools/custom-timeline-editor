using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DefaultNamespace;
using Editor;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Needle.Timeline
{
	[TrackClipType(typeof(CodeControlAsset))]
	[TrackBindingType(typeof(MonoBehaviour))]
	public class CodeTrack : TrackAsset
	{
		[SerializeField] private List<ClipInfo> clips = new List<ClipInfo>();

		[Serializable]
		public class ClipInfo
		{
			public string id;
			public AnimationClip clip;
			
			[NonSerialized]
			public List<IValueHandler> values;
		}

		private static BindingFlags DefaultFlags => BindingFlags.Default | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

		protected override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip clip)
		{
			Debug.Log("<b>Create Playable</b>");


			clip.CreateCurves("Procedural");
			var asset = clip.asset as CodeControlAsset;
			if (!asset) throw new NullReferenceException("Missing code control asset");
			ObjectIdentifier.TryGetObjectIdentifier(asset, out var objectIdentifier);
			var id = objectIdentifier.guid + "@" + objectIdentifier.localIdentifierInFile;

			foreach (var anim in gameObject.GetComponents<IAnimated>())
			{
				var model = clips.FirstOrDefault(clipInfo => clipInfo.id == id);
				if (model == null) model = new ClipInfo();
				if (model.id == null)
				{
					// Debug.Log("CREATE " + id);
					model.id = id;
					model.clip = clip.curves;
					clip.displayName += "\n" + id;
					clips.Add(model);
				}

				asset.clip = clip.curves;
				asset.info = model;

				if (model.values == null) model.values = new List<IValueHandler>();
				model.values.Clear();

				var fields = anim.GetType().GetFields(DefaultFlags);
				var type = anim.GetType();
				var clipBindings = AnimationUtility.GetCurveBindings(asset.clip);
				var path = AnimationUtility.CalculateTransformPath(gameObject.transform, null);
				Debug.Log(path);
				foreach (var field in fields)
				{
					if (field.FieldType != typeof(float)) continue;

					var handler = new FloatField(field, anim);
					model.values.Add(handler);
					
					var binding = clipBindings.FirstOrDefault(b => b.propertyName == field.Name);
					if (field.GetCustomAttribute<AnimateAttribute>() == null)
					{
						if (binding.propertyName != null)
						{
							Debug.Log("Remove " + binding.propertyName);
							AnimationUtility.SetEditorCurve(clip.curves, binding, null);
						}

						continue;
					}

					if (binding.propertyName == null)
					{
						Debug.Log("Create " + field.Name);
						model.clip.SetCurve(path, type, field.Name, new AnimationCurve());
					}
				}
			}

			// var curve = AnimationCurve.Linear(0, 0, 1, 1);
			return base.CreatePlayable(graph, gameObject, clip);
		}
		
		public class FloatField : IValueHandler
		{
			private readonly FieldInfo field;
			private readonly object instance;

			public FloatField(FieldInfo field, object instance)
			{
				this.field = field;
				this.instance = instance;
			}
			
			public void SetValue(object value)
			{
				field.SetValue(instance, value);
			}
		}
	}
}
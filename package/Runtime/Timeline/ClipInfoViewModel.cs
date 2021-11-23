using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

namespace Needle.Timeline
{
	public class ClipInfoViewModel : IReadClipTime
	{
		public static IReadOnlyList<ClipInfoViewModel> Instances => instances;
		public static IEnumerable<ClipInfoViewModel> ActiveInstances => 
			instances.Where(vm => vm.IsValid && vm.currentlyInClipTime && vm.timelineClip.asset);
		private static readonly List<ClipInfoViewModel> instances = new List<ClipInfoViewModel>();

		internal static void Register(ClipInfoViewModel vms)
		{
			if (!instances.Contains(vms)) instances.Add(vms);
		}

		internal static void Unregister(ClipInfoViewModel vm)
		{
			instances.Remove(vm);
		}

		internal static void RemoveInvalidInstances()
		{
			instances.RemoveAll(i => !i.IsValid);
		}

		private static void NotifyChanged(PlayableAsset asset)
		{
			var codeAsset = asset as CodeControlAsset;
			if (!codeAsset) return;
			var data = codeAsset.data; 
			foreach (var i in Instances)
			{
				if (i.asset == asset || i.asset is CodeControlAsset ca && ca.data == data)
				{
					i.RequiresReload = true;
				} 
			}
		}
		
		public static event Action<ClipInfoViewModel> Created;

		public bool HasUnsavedChanges { get; internal set; }
		internal bool RequiresReload { get; set; }

		internal PlayableDirector director;
		internal PlayableAsset asset;
		internal TimelineClip TimelineClip => timelineClip;
		internal bool failedLoading;

		private readonly ClipInfoModel model;
		private readonly TimelineClip timelineClip;
		private readonly CodeControlTrack track;
		
		private ClipInfoViewModel()
		{
			instances.Add(this);
			Created?.Invoke(this);
		}

		public ClipInfoViewModel(CodeControlTrack track, string name, IAnimated script, ClipInfoModel model, TimelineClip timelineClip) : this()
		{
			this.track = track;
			this.Name = name;
			this.Script = script;
			this.model = model;
			this.timelineClip = timelineClip;
		}

		internal void Clear()
		{
			foreach (var c in clips) c.Changed -= OnClipChanged;
			clips.Clear();
			values.Clear();
			storedValues.Clear();
		}

		internal bool Save(ILoader loader)
		{
			Debug.Log("<b>SAVE</b> " + Id + "@" + startTime.ToString("0.00")); 
			var context = new SerializationContext(TimelineClip, asset);
			var anySaved = false; 
			foreach (var clip in clips)
			{
				context.DisplayName = clip.Name;
				anySaved |= loader.Save(clip.Id, context, clip);
			}
			if(anySaved) 
				NotifyChanged(asset);
			return anySaved;
		}
		
		internal void Register(IValueHandler handler, ICustomClip clip)
		{
			values.Add(handler); 
			clips.Add(clip);
			clip.Changed += OnClipChanged;
		}

		/// <summary> 
		/// Called e.g. when clips are cloned
		/// </summary>
		internal void Replace(ICustomClip oldClip, ICustomClip newClip)
		{
			if (oldClip == newClip || oldClip == null || newClip == null) throw new Exception("Invalid op");
			for (var index = 0; index < clips.Count; index++)
			{
				var clip = clips[index];
				if (clip == oldClip)
				{
					clips[index] = newClip;
					oldClip.Changed -= OnClipChanged;
					newClip.Changed += OnClipChanged;
					break;
				}
			}
		}

		private void OnClipChanged(ICustomClip clip)
		{
			if (!track)
			{
				Debug.LogError("Clip does not exist anymore");
				clip.Changed -= OnClipChanged;
				return;
			}

			HasUnsavedChanges = true;
			
#if UNITY_EDITOR
			EditorUtility.SetDirty(track);
#endif
			// TODO: figure out if we really need this
			track.dirtyCount = (track.dirtyCount + 1) % uint.MaxValue;
			director.Evaluate();
#if UNITY_EDITOR
			TimelineWindowUtil.TryRepaint(); 
#endif
		}

		internal void OnProcessedFrame(FrameInfo info)
		{
			if (Script is IAnimatedEvents cb)
			{
				cb.OnEvaluated(info);
			}
		}

		internal AnimationClip AnimationClip => model.clip;

		public bool Solo
		{
			get => model.solo;
			set => model.solo = value;
		}

		public bool IsValid => director;
		public string Name { get; set; } 
		public string Id => model.id;
		public IAnimated Script { get; internal set; }
		public readonly List<IValueHandler> values = new List<IValueHandler>();
		public readonly List<ICustomClip> clips = new List<ICustomClip>();
		public double startTime => timelineClip.start;
		public double endTime => timelineClip.end;
		public double length => timelineClip.duration;
		public double timeScale => timelineClip.timeScale;
		public double currentTime => director.time;
		public double clipTime => (currentTime - startTime) * timeScale;
		public double clipLength => length * timeScale;
		public bool currentlyInClipTime => clipTime >= 0 && clipTime <= clipLength;
		public double ToClipTime(double time) => (time - startTime) * timeScale;
		public double ClipTime => clipTime;


		private readonly List<(ICustomClip clip, IValueHandler handler, object value)> storedValues 
			= new List<(ICustomClip clip, IValueHandler handler, object value)>();

		internal void StoreEvaluatedResult(IValueHandler handler, ICustomClip clip, object value)
		{
			var index = storedValues.FindIndex(d => d.clip == clip);
			if (index < 0 || index >= storedValues.Count)
			{
				storedValues.Add(default);
				index = storedValues.Count - 1;
			}
			var existing = storedValues[index];
			existing.clip = clip;
			existing.handler = handler;
			existing.value = value;
			storedValues[index] = existing;
		}

		internal void RenderDataPreview(bool renderOnionskin)
		{
			if (!IsValid) return;
			if (renderOnionskin) 
			{
				renderer ??= new OnionSkinRenderer(this);
				renderer.Render();
				foreach (var stored in storedValues)
				{
					stored.handler.SetValue(stored.value);
				}
			}
			if (Script is IOnionSkin on)
				on.RenderOnionSkin(OnionData.Default);
		}

		private OnionSkinRenderer renderer;
	}
}
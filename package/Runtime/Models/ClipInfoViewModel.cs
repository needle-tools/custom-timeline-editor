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

		// private static readonly List<ClipInfoViewModel> _lastActiveInstances = new List<ClipInfoViewModel>();
		public static IEnumerable<ClipInfoViewModel> ActiveInstances => instances.Where(vm => vm.IsValid && vm.currentlyInClipTime && vm.timelineClip.asset);

		public static event Action<ClipInfoViewModel> Created;

		private static readonly List<ClipInfoViewModel> instances = new List<ClipInfoViewModel>();


		private readonly ClipInfoModel model;
		private readonly TimelineClip timelineClip;
		internal PlayableDirector director;

		private ClipInfoViewModel()
		{
			instances.Add(this);
			Created?.Invoke(this);
		}

		public ClipInfoViewModel(string name, IAnimated script, ClipInfoModel model, TimelineClip timelineClip) : this()
		{
			this.Name = name;
			this.Script = script;
			this.model = model;
			this.timelineClip = timelineClip;
		}

		internal void Register(IValueHandler handler, ICustomClip clip)
		{
			values.Add(handler);
			clips.Add(clip);
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
		public readonly IAnimated Script;
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


		private List<(ICustomClip clip, IValueHandler handler, object value)>
			storedValues = new List<(ICustomClip clip, IValueHandler handler, object value)>();

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

		internal void RenderOnionSkin()
		{
			if (!IsValid) return;
			if (!(Script is IOnionSkin onion)) return;
			var time = (float)ClipTime;

			var renderPrev = false;
			for (var index = 0; index < clips.Count; index++)
			{
				var clip = clips[index];
				var prev = default(IReadonlyCustomKeyframe);
				foreach (var kf in clip.Keyframes)
				{
					var diff = kf.time - time; 
					if (diff < 0 && diff < -Mathf.Epsilon)
					{
						prev = kf;
					}
					else if (kf.time >= time)
					{
						if (prev != null)
						{
							renderPrev = true;
							values[index].SetValue(prev.value);
							break;
						}
					}
				}
			}

			if (renderPrev)
				onion.RenderOnionSkin(new OnionData(-1));

			var renderNext = false;
			for (var index = 0; index < clips.Count; index++)
			{
				var clip = clips[index];
				foreach (var kf in clip.Keyframes)
				{
					if (kf.time > time)
					{
						renderNext = true;
						values[index].SetValue(kf.value);
						break;
					}
				}
			}
			if (renderNext)
				onion.RenderOnionSkin(new OnionData(1));

			foreach (var stored in storedValues)
			{
				stored.handler.SetValue(stored.value);
			}
		}
	}
}
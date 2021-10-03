﻿using System;
using System.Collections.Generic;
using System.Linq;
using Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Needle.Timeline
{
	public class ClipInfoViewModel
	{
		public static IReadOnlyList<ClipInfoViewModel> Instances => instances;
		// private static readonly List<ClipInfoViewModel> _lastActiveInstances = new List<ClipInfoViewModel>();
 		public static IEnumerable<ClipInfoViewModel> ActiveInstances => instances.Where(vm => vm.currentlyInClipTime);

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
	}
}
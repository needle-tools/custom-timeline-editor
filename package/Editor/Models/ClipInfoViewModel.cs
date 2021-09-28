using System;
using System.Collections.Generic;
using System.Linq;
using Editor;
using UnityEngine;
using UnityEngine.Playables;

namespace Needle.Timeline
{
	public class ClipInfoViewModel
	{
		private static readonly List<ClipInfoViewModel> instances = new List<ClipInfoViewModel>();
		public static IReadOnlyList<ClipInfoViewModel> Instances => instances;
		public static event Action<ClipInfoViewModel> Created;

		private readonly ClipInfoModel model;
		internal PlayableDirector director;

		private ClipInfoViewModel()
		{
			instances.Add(this);
			Created?.Invoke(this);
		}

		public ClipInfoViewModel(string name, IAnimated script, ClipInfoModel model) : this()
		{
			this.Name = name;
			this.Script = script;
			this.model = model;
		}

		internal void Register(IValueHandler handler, ICustomClip clip)
		{
			values.Add(handler);
			clips.Add(clip);
		}

		internal AnimationClip AnimationClip => model.clip;

		public string Name { get; set; }
		public string Id => model.id;
		public readonly IAnimated Script;
		public readonly List<IValueHandler> values = new List<IValueHandler>();
		public readonly List<ICustomClip> clips = new List<ICustomClip>();
		public double startTime, endTime, length, timeScale;
		public double currentTime => director.time;
		public double clipTime => (currentTime - startTime) * timeScale;
		public double clipLength => length * timeScale;
		public bool currentlyInClipTime => clipTime >= 0 && clipTime <= clipLength;
		public double ToClipTime(double time) => (time - startTime) * timeScale;
	}
}
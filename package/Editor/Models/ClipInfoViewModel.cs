using System.Collections.Generic;
using Editor;
using UnityEngine;

namespace Needle.Timeline
{
	public class ClipInfoViewModel
	{
		private readonly ClipInfoModel model;

		public ClipInfoViewModel(ClipInfoModel model)
		{
			this.model = model;
		}

		internal void Register(IValueHandler handler, ICustomClip clip)
		{
			values.Add(handler);
			clips.Add(clip);
		}

		internal AnimationClip AnimationClip => model.clip;

		public readonly List<IValueHandler> values = new List<IValueHandler>();
		public readonly List<ICustomClip> clips = new List<ICustomClip>();
	}
}
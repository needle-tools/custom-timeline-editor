#nullable enable

using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Needle.Timeline
{
	[Serializable]
	public class ClipInfoModel
	{
		public string id;
		public AnimationClip? clip;

		public ClipInfoModel(string id, AnimationClip? clip)
		{
			this.id = id;
			this.clip = clip;
		}
	}
}
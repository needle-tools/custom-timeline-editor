#nullable enable

using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Needle.Timeline
{
	[Serializable]
	public class ClipInfoModel
	{
		public string id;
		public AnimationClip? clip;
		public bool solo;

		public ClipInfoModel(string id, AnimationClip? clip)
		{
			this.id = id;
			this.clip = clip;
		}
	}
}
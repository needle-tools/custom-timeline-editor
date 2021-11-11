using System;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Needle.Timeline
{
	public class SerializationContext : ISerializationContext
	{
		public string DisplayName { get; set; }
		public TimelineClip Clip { get; }
		public Type Type { get; set; }
		public PlayableAsset Asset { get; }

		public SerializationContext(TimelineClip clip, PlayableAsset asset)
		{
			Clip = clip;
			Asset = asset;
		}
	}
}
#nullable enable
using System;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Needle.Timeline
{
	public interface ISerializationContext
	{
		string? DisplayName { get; }
		TimelineClip Clip { get; }	
		Type Type { get; }
		PlayableAsset Asset { get; }
	}
}
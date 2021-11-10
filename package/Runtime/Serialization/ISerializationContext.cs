#nullable enable
using System;
using JetBrains.Annotations;
using UnityEngine.Timeline;

namespace Needle.Timeline
{
	public interface ISerializationContext
	{
		string? DisplayName { get; }
		TimelineClip Clip { get; }	
		Type Type { get; }
	}
}
using System;
using UnityEngine.Timeline;

namespace Needle.Timeline
{
	public interface ISerializationContext
	{
		TimelineClip Clip { get; }	
		Type Type { get; }
	}
}
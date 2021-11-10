using System;
using UnityEngine.Timeline;

namespace Needle.Timeline
{
	public class SerializationContext : ISerializationContext
	{
		public string DisplayName { get; set; }
		public TimelineClip Clip { get; private set; }
		public Type Type { get; set; }

		public SerializationContext(TimelineClip clip)
		{
			Clip = clip;
		}
	}
}
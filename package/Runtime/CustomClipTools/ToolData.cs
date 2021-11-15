#nullable enable
using System;
using UnityEngine;

namespace Needle.Timeline
{
	public readonly struct ToolData
	{
		public readonly ICustomClip Clip;
		public readonly float Time;
		
		/// <summary>
		/// The targeted (owner) object
		/// </summary>
		public readonly object Object;

		public readonly ITimelineContext TimelineContext;
		
		public ToolData(object @object, ICustomClip clip, float time, ITimelineContext timelineContext)
		{
			Object = @object;
			Clip = clip;
			Time = time;
			TimelineContext = timelineContext;
		}
	}
}
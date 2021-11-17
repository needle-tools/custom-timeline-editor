#nullable enable
using System;
using UnityEngine;

namespace Needle.Timeline
{
	public readonly struct ToolData
	{
		public readonly ICustomClip Clip;
		public readonly int ClipHash;
		public readonly float Time;
		
		/// <summary>
		/// The targeted (owner) object
		/// </summary>
		public readonly object Object;

		public readonly ITimelineContext TimelineContext;
		public readonly IInputCommandHandler CommandHandler;
		
		public ToolData(object @object, ICustomClip clip, float time, ITimelineContext timelineContext, IInputCommandHandler commandHandler)
		{
			Object = @object;
			Clip = clip;
			Time = time;
			TimelineContext = timelineContext;
			CommandHandler = commandHandler;
			ClipHash = Clip.GetHashCode();
		}
	}
}
using JetBrains.Annotations;
using Needle.Timeline.Commands;
#nullable enable

namespace Needle.Timeline
{
	public interface ITimelineContext
	{
		float TimeF { get; }
		double Time { get; }
		TimelineModifyTime? SetTime(float time);
		TimelineModifyTime GetTimeCommand();
	}
}
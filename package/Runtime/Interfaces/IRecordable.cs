using System;

namespace Needle.Timeline
{
	public interface IRecordable
	{
		bool IsRecording { get; set; }
		event Action RecordingStateChanged;
	}
}
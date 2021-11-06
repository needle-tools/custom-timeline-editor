using System;

namespace Needle.Timeline
{
	public interface IEnabled
	{
		bool Enabled { get; set; }
		event Action EnabledChanged;
	}
}
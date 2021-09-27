using System;
using System.Collections.Generic;

namespace Needle.Timeline
{
	public interface ICustomClipTool
	{
		ICustomClip ActiveClip { get; set; }
		ClipInfoViewModel ViewModel { set; }
		bool Supports(Type type);
	}
}
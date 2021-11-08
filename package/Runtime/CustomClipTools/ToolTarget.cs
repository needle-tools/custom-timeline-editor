using System.Collections.Generic;

namespace Needle.Timeline
{
	public class ToolTarget
	{
		// private to limit binding stuff even more with timeline
		private readonly ClipInfoViewModel viewModel;
		
		public readonly ICustomClip Clip;
		public bool IsNull() => Clip == null;
		public double Time => viewModel?.clipTime ?? 0;
		public float TimeF => (float)(viewModel?.clipTime ?? 0f);

		public ToolTarget(ClipInfoViewModel viewModel, ICustomClip clip)
		{
			this.viewModel = viewModel;
			Clip = clip;
		}
	}
}
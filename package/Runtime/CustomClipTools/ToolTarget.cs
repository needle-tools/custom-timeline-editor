using System.Collections.Generic;
using UnityEngine.Playables;

namespace Needle.Timeline
{
	public class ToolTarget
	{
		// private to limit binding stuff even more with timeline
		private readonly ClipInfoViewModel viewModel;

		/// <summary>
		/// Target instance
		/// </summary>
		public object Object => viewModel.Script;
		public readonly ICustomClip Clip;
		public bool IsNull() => Clip == null;
		public double Time => viewModel?.clipTime ?? 0;
		public float TimeF => (float)(viewModel?.clipTime ?? 0f);
		
		public void EnsurePaused()
		{
			if(viewModel.director.state == PlayState.Playing)
				viewModel.director.Pause();
		}

		public ToolTarget(ClipInfoViewModel viewModel, ICustomClip clip)
		{
			this.viewModel = viewModel;
			Clip = clip;
		}
	}
}
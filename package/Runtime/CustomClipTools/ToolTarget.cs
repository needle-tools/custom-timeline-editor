namespace Needle.Timeline
{
	public readonly struct ToolTarget
	{
		public readonly ClipInfoViewModel ViewModel;
		public readonly ICustomClip Clip;
		public bool IsNull() => Clip == null;
		public double Time => ViewModel?.clipTime ?? 0;
		public float TimeF => (float)(ViewModel?.clipTime ?? 0f);

		public ToolTarget(ClipInfoViewModel viewModel, ICustomClip clip)
		{
			ViewModel = viewModel;
			Clip = clip;
		}
	}
}
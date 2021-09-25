namespace Needle.Timeline
{
	public interface ICustomClipTool
	{
		ICustomClip ActiveClip { get; set; }
		ClipInfoViewModel ViewModel { set; }
	}
}
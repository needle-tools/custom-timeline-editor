namespace Needle.Timeline
{
	public interface IHasEasing
	{
		ICurveEasing DefaultEasing { get; set; }
	}
}
using Unity.Burst;

namespace Needle.Timeline
{
	public interface IFloatModifier
	{
		float Modify(float value);
	}
}
using System.Collections.Generic;

namespace Needle.Timeline
{
	public interface IKeyframesProvider
	{
		IEnumerable<ICustomKeyframe> Keyframes { get; }
	}
	
	public interface IKeyframesProvider<T> : IKeyframesProvider
	{
		new IEnumerable<ICustomKeyframe<T>> Keyframes { get; }
	}
}
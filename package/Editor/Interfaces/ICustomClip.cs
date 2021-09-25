using System;

namespace Needle.Timeline
{
	public interface ICustomClip : IInterpolator
	{
		string Name { get; set; }
		object Evaluate(float time);
		bool Add(ICustomKeyframe kf);
		void Remove(ICustomKeyframe kf);
		event Action Changed;
	}

	public interface ICustomClip<out T> : ICustomClip
	{
		new T Evaluate(float time);
	}
}
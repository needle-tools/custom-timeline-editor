using System;
using System.Collections.Generic;

namespace Needle.Timeline
{
	public interface ICustomClip : IInterpolator
	{
		string Name { get; set; }
		object Evaluate(float time);
		bool CanAdd(Type type);
		bool CanAdd(ICustomKeyframe kf);
		bool Add(ICustomKeyframe kf);
		void Remove(ICustomKeyframe kf);
		event Action Changed;
		IReadOnlyCollection<IReadonlyCustomKeyframe> Keyframes { get; }
		ICustomKeyframe GetPrevious(float time);
		ICustomKeyframe GetClosest(float time);
		Type[] SupportedTypes { get; }
	}

	public interface ICustomClip<out T> : ICustomClip
	{
		new T Evaluate(float time);
	}
}
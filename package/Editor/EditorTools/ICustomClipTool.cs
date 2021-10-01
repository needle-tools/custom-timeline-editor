using System;

namespace Needle.Timeline
{
	public interface ICustomClipTool
	{
		void AddTarget(ClipInfoViewModel vm, ICustomClip clip);
		void RemoveTarget(ICustomClip clip);
		void RemoveAllTargets();
		bool HasClipTarget(Type clipType);
		bool Supports(Type type);
	}
}
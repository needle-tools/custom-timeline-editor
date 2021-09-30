using System;

namespace Needle.Timeline
{
	public interface ICustomClipTool
	{
		void Add(ClipInfoViewModel vm, ICustomClip clip);
		void Remove(ICustomClip clip);
		void Clear();
		bool ContainsClip(Type clipType);
		bool Supports(Type type);
	}
}
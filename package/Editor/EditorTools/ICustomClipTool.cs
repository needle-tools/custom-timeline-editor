using System;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	public interface ICustomClipTool
	{
		internal void AddTarget(ClipInfoViewModel vm, ICustomClip clip);
		internal void RemoveTarget(ICustomClip clip);
		internal void RemoveAllTargets();
		internal bool HasClipTarget(Type clipType);
		internal void Attach(VisualElement el);
		internal void Detach(VisualElement el);

		bool Supports(Type type);
	}
}
using System;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	public interface IControlBuilder
	{
		bool CanBuild(Type type);
		VisualElement Build(Type type, IViewValueHandler binding);
	}
}
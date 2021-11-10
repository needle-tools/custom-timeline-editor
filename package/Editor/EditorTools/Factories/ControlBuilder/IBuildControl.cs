#nullable enable

using System;
using JetBrains.Annotations;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	public interface IControlBuilder
	{
		bool CanBuild(Type type);
		VisualElement? Build(Type type, IViewValueHandler viewValue, IContext? context = null);
	}

	public interface IContext
	{
		IHasCustomAttributes? Attributes { get; }
	}
}
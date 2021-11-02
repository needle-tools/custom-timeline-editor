using System;
using System.Collections.Generic;
using UnityEngine;
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
		internal void GetOrCreateSettings(ref ScriptableObject obj);
		internal bool IsValid { get; }

		bool Supports(Type type);
		
		IReadOnlyList<ToolTarget> Targets { get; }
	}
}
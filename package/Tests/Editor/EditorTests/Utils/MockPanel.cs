using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Needle.Timeline.Tests
{
	internal class MockPanel : IPanel
	{
		public void Dispose()
		{
			throw new System.NotImplementedException();
		}

		public VisualElement Pick(Vector2 point)
		{
			throw new System.NotImplementedException();
		}

		public VisualElement PickAll(Vector2 point, List<VisualElement> picked)
		{
			throw new System.NotImplementedException();
		}

		public VisualElement visualTree { get; }
		public EventDispatcher dispatcher { get; }
		public ContextType contextType { get; }
		public FocusController focusController { get; }
		public ContextualMenuManager contextualMenuManager { get; }
	}
}
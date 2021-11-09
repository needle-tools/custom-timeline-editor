using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Needle.Timeline.Tests
{
	internal class MockWindow : EditorWindow
	{
		internal static void RunInWindow(Action<EditorWindow> b)
		{
			var window = CreateInstance<MockWindow>();
			try
			{
				Assert.NotNull(window);
				window.Show();
				b(window);
			}
			finally
			{
				Assert.NotNull(window);
				window.Close();
				DestroyImmediate(window);
			}
		}
	}

	internal class InWindowContext : IDisposable
	{
		private readonly EditorWindow Window;
		
		public InWindowContext(EditorWindow window = null)
		{
			Window = window;
			if(!Window)
				Window = ScriptableObject.CreateInstance<MockWindow>();
			Window.Show(true);
		}

		public void Add(VisualElement el) => Window.rootVisualElement.Add(el);
		public VisualElement VisualElement => Window.rootVisualElement;
		
		public void Dispose()
		{
			if (Window)
			{
				Window.Close();
				Object.DestroyImmediate(Window);
			}
		}
	}
}
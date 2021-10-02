using System.Linq;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	public class ToolButtonManipulator : Manipulator
	{
		private readonly ICustomClipTool tool;

		public ToolButtonManipulator(ICustomClipTool tool)
		{
			this.tool = tool;
		}

		protected override void RegisterCallbacksOnTarget()
		{
			if (target is Button btn)
			{
				btn.clicked += OnClicked;
			}
		}

		protected override void UnregisterCallbacksFromTarget()
		{
			if (target is Button btn)
			{
				btn.clicked -= OnClicked;
			}
		}

		private void OnClicked()
		{
			ToolsHandler.DeselectAll();
			ToolsHandler.Select(tool);
		}
	}
}
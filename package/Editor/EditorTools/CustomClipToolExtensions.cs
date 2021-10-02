namespace Needle.Timeline
{
	public static class CustomClipToolExtensions
	{
		public static void Deselect(this ICustomClipTool tool)
		{
			ToolsHandler.Deselect(tool);
		}

		public static void Select(this ICustomClipTool tool)
		{
			ToolsHandler.Select(tool);
		}

		public static bool IsSelected(this ICustomClipTool tool)
		{
			return ToolsHandler.IsSelected(tool);
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace Needle.Timeline
{
	public static class ToolsHandler
	{
		private static ClipInfoViewModel viewModel;
		private static IReadOnlyList<ICustomClip> clips;

		private static List<ICustomClipTool> tools;
		private static readonly List<ICustomClipTool> availableTools = new List<ICustomClipTool>();

		internal static void OnEnable(CodeControlAsset track)
		{
			clips = track?.viewModel?.clips;
			viewModel = track?.viewModel;

			if (tools == null)
			{
				tools = new List<ICustomClipTool>();
				var toolTypes = TypeCache.GetTypesDerivedFrom<ICustomClipTool>();
				foreach (var tool in toolTypes)
				{
					if (tool.IsAbstract || tool.IsInterface) continue;
					var instance = (ICustomClipTool) Activator.CreateInstance(tool);
					tools.Add(instance);
				}
			}
			
			availableTools.Clear();
			availableTools.AddRange(tools);
		}

		internal static void OnDisable()
		{
			clips = null;
			viewModel = null;
			// if(typeof(ICustomClipTool).IsAssignableFrom(ToolManager.activeToolType))
			// 	ToolManager.RestorePreviousTool();
		}

		internal static void OnInspectorGUI()
		{
			if (clips == null) return;
			foreach (var tool in tools)
			{
				var name = tool.GetType().Name;
				if (GUILayout.Button(name, GUILayout.Height(30)))
				{
					tool.ViewModel = viewModel;
					tool.ActiveClip = clips[1];
					if (tool is EditorTool et)
					{
						ToolManager.SetActiveTool(et);
					}
				}
			}
		}

		internal static void OnSceneGUI()
		{
			
		}
	}
}
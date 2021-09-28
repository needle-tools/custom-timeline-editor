using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	public static class ToolsHandler
	{
		[InitializeOnLoadMethod]
		private static void Init()
		{
			OnSetupTools();
			ClipInfoViewModel.Created += OnCreated;
			SceneView.beforeSceneGui += OnSceneGui;
			EditorSceneManager.sceneClosing += OnUnload;
		}

		private static void OnUnload(Scene scene, bool rem)
		{
			_recreateTools = true;
		}

		private static void OnCreated(ClipInfoViewModel obj)
		{
			_recreateTools = true;
		}

		private static VisualElement _root, _tools;
		private static bool _recreateTools;
		private static List<ICustomClipTool> _toolInstances;
		private static readonly List<SceneView> _scenes = new List<SceneView>();

		private static void OnSetupTools()
		{
			if (_toolInstances != null) return;
			_toolInstances = new List<ICustomClipTool>();
			var toolTypes = TypeCache.GetTypesDerivedFrom<ICustomClipTool>();
			foreach (var tool in toolTypes)
			{
				if (tool.IsAbstract || tool.IsInterface) continue;
				var instance = (ICustomClipTool)Activator.CreateInstance(tool);
				_toolInstances.Add(instance);
			}
		}

		private static void OnSceneGui(SceneView obj)
		{
			OnBuildGUIIfNecessary();
			CreateButtonsIfNecessary();

			if (!_scenes.Contains(obj))
			{
				_scenes.Add(obj);
				obj.rootVisualElement.Add(_root);
				obj.rootVisualElement.style.flexDirection = FlexDirection.ColumnReverse;
			}
		}

		private static void OnBuildGUIIfNecessary()
		{
			if (_root != null) return;
			_root = new VisualElement();
			_root.style.minWidth = 20;
			_root.style.maxWidth = new StyleLength(new Length(50, LengthUnit.Percent));
			var backgroundColor = EditorGUIUtility.isProSkin
				? new Color(0.21f, 0.21f, 0.21f, 0.8f)
				: new Color(0.8f, 0.8f, 0.8f, 0.8f);
			_root.style.backgroundColor = backgroundColor;
			_root.style.marginLeft = 10f;
			_root.style.marginBottom = 10f;
			_root.style.paddingTop = 5f;
			_root.style.paddingRight = 5f;
			_root.style.paddingLeft = 5f;
			_root.style.paddingBottom = 5f;

			_tools = new VisualElement();
			_root.Add(_tools);
		}

		private static void CreateButtonsIfNecessary()
		{
			if (!_recreateTools) return;
			_recreateTools = false;
			_tools.Clear();
			Debug.Log("Create Tools");

			foreach (var viewModel in ClipInfoViewModel.Instances)
			{
				var vm = new VisualElement();
				vm.name = viewModel.Name;
				vm.style.paddingBottom = 5;
				_tools.Add(vm);

				vm.schedule.Execute(() =>
				{
					if (!viewModel.IsValid)
					{
						Debug.Log("Removing stuff");
						vm.RemoveFromHierarchy();
						return;
					}
					var inRange = viewModel.currentlyInClipTime;
					vm.style.visibility = inRange ? Visibility.Visible : Visibility.Hidden;
					vm.style.display = inRange ? DisplayStyle.Flex : DisplayStyle.None;
				}).Every(500);
				
				var title = new Label(viewModel.Script.ToString());
				vm.Add(title);

				var toolsElement = new VisualElement();
				toolsElement.style.alignItems = new StyleEnum<Align>(Align.FlexStart);
				toolsElement.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
				vm.Add(toolsElement);
				
				foreach (var clip in viewModel.clips)
				{
					var hasToolSupport = false;
					foreach (var tool in _toolInstances)
					{
						if (!clip.SupportedTypes.Any(tool.Supports)) continue;
						if (!hasToolSupport)
						{
						}
						hasToolSupport = true;
						var name = tool.GetType().Name;
						var toolButton = new Button();
						toolButton.style.height = 24f;
						toolButton.text = name;
						toolButton.style.flexGrow = 0;

						void OnToolButtonClicked()
						{
							tool.ViewModel = viewModel;
							tool.ActiveClip = clip;
							if (tool is EditorTool et)
							{
								Debug.Log("Activate tool " + et + ", " + tool);
								ToolManager.SetActiveTool(et);
							} 
						}

			
						toolButton.RegisterCallback<AttachToPanelEvent>(evt =>
						{ 
							Debug.Log("Attach button");
							toolButton.clicked += OnToolButtonClicked;
						});
						toolButton.RegisterCallback<DetachFromPanelEvent>(evt =>
						{
							Debug.Log("Goodbye button");
							toolButton.clicked -= OnToolButtonClicked;
						});
						toolsElement.Add(toolButton);
					}
				}
			}
		}

		private class Man : Manipulator
		{
			protected override void RegisterCallbacksOnTarget()
			{
				
			}

			protected override void UnregisterCallbacksFromTarget()
			{
			}
		}
	}
}
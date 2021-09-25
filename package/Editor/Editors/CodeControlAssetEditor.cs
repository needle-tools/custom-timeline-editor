using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Needle.Timeline
{
	[CustomEditor(typeof(CodeControlAsset))]
	public class CodeControlAssetEditor : UnityEditor.Editor
	{
		private async void OnEnable()
		{
			var asset = target as CodeControlAsset;
			// workaround until asset not being initialized automatically when reload happens
			if (asset?.viewModel == null) await Task.Delay(1);
			ToolsHandler.OnEnable(asset);
		}
		
		private void OnDisable()
		{
			ToolsHandler.OnDisable();
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			ToolsHandler.OnInspectorGUI();
		}

		private void OnSceneGUI()
		{
			var script = target as CodeControlAsset;
			ToolsHandler.OnSceneGUI();
			
			// var cam = Camera.current;
			// var mp = Event.current.mousePosition;
			// mp.y = Screen.height - mp.y;
			// var ray = cam.ScreenPointToRay(mp, Camera.MonoOrStereoscopicEye.Mono);
			// var plane = new Plane(Vector3.up, Vector3.zero);
			// if (plane.Raycast(ray, out var enter))
			// {
			// 	var hitPoint = ray.GetPoint(enter);
			// 	var radius = .2f;
			// 	Handles.DrawWireDisc(hitPoint, Vector3.up, radius, 1);
			// 	// disable selection rectangle
			// 	if (Event.current.modifiers == EventModifiers.None)
			// 	{
			// 		if (Event.current.type != EventType.Layout && Event.current.type != EventType.Repaint)
			// 		{
			// 			switch (Event.current.type)
			// 			{
			// 				case EventType.MouseDrag:
			// 				case EventType.MouseDown:
			// 					GUIUtility.hotControl = 0;
			// 					// script.points.Add(Random.insideUnitSphere * radius + hitPoint);
			// 					break;
			// 			}
			// 		}
			// 	}
			// } 
		}
	}
}
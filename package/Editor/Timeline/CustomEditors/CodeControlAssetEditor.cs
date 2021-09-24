using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Needle.Timeline
{
	[CustomEditor(typeof(CodeControlAsset))]
	public class CodeControlAssetEditor : UnityEditor.Editor
	{
		// private void OnEnable()
		// {
		// 	ToolManager.SetActiveTool(typeof(PlatformTool));
		// }
		//
		// private void OnDisable()
		// {
		// 	ToolManager.RestorePreviousTool();
		// }

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			var asset = target as CodeControlAsset;
			if (!asset) return;

			if (asset.viewModel != null && asset.viewModel.AnimationClip != null)
			{
				if (GUILayout.Button("Add keyframe"))
				{
					var clip = asset.viewModel.AnimationClip;
					var clipBindings = AnimationUtility.GetCurveBindings(clip);
					for (var index = 0; index < clipBindings.Length; index++)
					{
						var binding = clipBindings[index];
						var curve = AnimationUtility.GetEditorCurve(clip, binding);
						curve.AddKey(Random.value, Random.value);
						AnimationUtility.SetEditorCurve(clip, binding, curve);
					}
				}
			}
		}

		private void OnSceneGUI()
		{
			var script = target as CodeControlAsset;
			
			var cam = Camera.current;
			var mp = Event.current.mousePosition;
			mp.y = Screen.height - mp.y;
			var ray = cam.ScreenPointToRay(mp, Camera.MonoOrStereoscopicEye.Mono);
			var plane = new Plane(Vector3.up, Vector3.zero);
			if (plane.Raycast(ray, out var enter))
			{
				var hitPoint = ray.GetPoint(enter);
				var radius = .2f;
				Handles.DrawWireDisc(hitPoint, Vector3.up, radius, 1);
				// disable selection rectangle
				if (Event.current.modifiers == EventModifiers.None)
				{
					if (Event.current.type != EventType.Layout && Event.current.type != EventType.Repaint)
					{
						switch (Event.current.type)
						{
							case EventType.MouseDrag:
							case EventType.MouseDown:
								GUIUtility.hotControl = 0;
								// script.points.Add(Random.insideUnitSphere * radius + hitPoint);
								break;
						}
					}
				}
			} 
		}
	}
}
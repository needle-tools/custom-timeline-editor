// using System;
// using DefaultNamespace;
// using UnityEditor;
// using UnityEngine;
// using Random = UnityEngine.Random;
//
// namespace _Sample._Sample
// {
// 	[CustomEditor(typeof(AnimatedScript))]
// 	public class AnimatedEditor : UnityEditor.Editor
// 	{
// 		
// 		// private void OnSceneGUI()
// 		// {
// 		// 	// var script = target as AnimatedScript;
// 		// 	//
// 		// 	// var cam = Camera.current;
// 		// 	// var mp = Event.current.mousePosition;
// 		// 	// mp.y = Screen.height - mp.y;
// 		// 	// var ray = cam.ScreenPointToRay(mp, Camera.MonoOrStereoscopicEye.Mono);
// 		// 	// var plane = new Plane(Vector3.up, Vector3.zero);
// 		// 	// if (plane.Raycast(ray, out var enter))
// 		// 	// {
// 		// 	// 	var hitPoint = ray.GetPoint(enter);
// 		// 	// 	var radius = .2f;
// 		// 	// 	Handles.DrawWireDisc(hitPoint, Vector3.up, radius, 1);
// 		// 	// 	// disable selection rectangle
// 		// 	// 	if (Event.current.modifiers == EventModifiers.None)
// 		// 	// 	{
// 		// 	// 		if (Event.current.type != EventType.Layout && Event.current.type != EventType.Repaint)
// 		// 	// 		{
// 		// 	// 			switch (Event.current.type)
// 		// 	// 			{
// 		// 	// 				case EventType.MouseDrag:
// 		// 	// 				case EventType.MouseDown:
// 		// 	// 					GUIUtility.hotControl = 0;
// 		// 	// 					script.points.Add(Random.insideUnitSphere * radius + hitPoint);
// 		// 	// 					break;
// 		// 	// 			}
// 		// 	// 		}
// 		// 	// 	}
// 		// 	// } 
// 		// }
// 	}
// }
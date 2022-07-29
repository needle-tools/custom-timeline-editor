using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Codice.Client.Common;
using Needle.Timeline;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SimpleScript : MonoBehaviour, IAnimated
{
	public class Point
	{
		public Vector3 Position;
		public float Radius = .1f;
		public Color Color = Color.white;
	}

	[Animate] public List<Point> MyList;

	private void OnDrawGizmos()
	{
		if (MyList == null) return;
		foreach (var pt in MyList)
		{
			Gizmos.color = pt.Color;
			Gizmos.DrawSphere(pt.Position, pt.Radius);
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(SimpleScript))]
	private class SimpleScriptEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			EditorGUILayout.HelpBox("Hey, thanks for trying out Needle Custom Timeline.\n\n" +
			                        "This script is a simple example of what you need to implement to start animating. It's basically just one interface (just for marking) and then tag the fields you want to animate using [Animate].\n\n" +
			                        "In the timeline window you can open the curve view to see keyframes.\n" +
			                        "Double click a keyframe to jump to its time.\n" +
			                        "Use the tools in Tools/Timeline Tools for painting and manipulating data (make sure to click the red record button first.", MessageType.None);
			
			if(GUILayout.Button("Join Discord"))
				Application.OpenURL("https://discord.needle.tools");
		}
	}
#endif
}
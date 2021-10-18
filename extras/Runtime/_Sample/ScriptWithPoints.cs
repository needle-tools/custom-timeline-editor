#nullable enable
using System;
using System.Collections.Generic;
using Needle.Timeline;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Serialization;
using Handles = UnityEditor.Handles;

namespace _Sample._Sample
{
	public class ScriptWithPoints : MonoBehaviour, IAnimated, IOnionSkin
	{
		[Animate] public List<Point>? Points = new List<Point>();

		public struct Point : ICreationCallbacks
		{
			public Vector3 Position;
			public float Weight;
			public Color Color;

			public void Init(CreationStage stage, IToolData? _)
			{
				Weight = .05f;
				Color = Color.white; 
			}
		}
		

		[Animate]
		public List<Direction>? Lines = new List<Direction>();

		[Animate]
		public List<Circle>? Circles = new List<Circle>();

		public struct Circle : ICreationCallbacks, IReceiveInput
		{
			public Vector3 Position;
			public float Radius;
			
			public void Init(CreationStage stage, IToolData? data)
			{
				if (data == null) return;
				if(data.WorldPosition != null && data.StartWorldPosition != null)
					Radius = (data.WorldPosition.Value - data.StartWorldPosition.Value).magnitude * .1f;
			}

			public bool OnInput(IToolData data)
			{
				var sp = data.ToScreenPoint(Position);
				var dist = (sp - data.ScreenPosition).magnitude;
				// Handles.Label(Position, sp + "\n" + dist + "\n" + data.ScreenPosition + "\n" + Screen.height);
				if (dist > Screen.width * .3f) return false;
				Handles.color = new Color(0, 1, 1, .5f);
				var newRadius = Handles.RadiusHandle(Quaternion.LookRotation(Camera.current.transform.forward), Position, Radius, true);
				var changed = Math.Abs(newRadius - Radius) > Mathf.Epsilon;
				Radius = newRadius;
				return changed;
			}
		}

		private void OnDrawGizmos()
		{
			RenderOnionSkin(0);
		}

		public void RenderOnionSkin(int layer)
		{
			var onionColor = new Color(1, 1, 1, .3f);
			var onionColor01 = 0f;
			if (layer != 0)
			{
				onionColor01 = 1f;
				if(layer < 0)
					onionColor = new Color(1f, .5f, .5f, .3f);
				else
					onionColor = new Color(0.5f, 1f, .5f, .3f);
			}
			
			if (Points != null)
			{
				foreach (var pt in Points)
				{
					Gizmos.color = Color.Lerp(pt.Color, onionColor, onionColor01);
					Gizmos.DrawSphere(pt.Position, pt.Weight + .01f);
				}
			}

			if (Lines != null)
			{
				Gizmos.color = Color.Lerp(Color.white, onionColor, onionColor01);
				foreach (var line in Lines)
				{
					line.DrawGizmos();
				}
			}

#if UNITY_EDITOR
			if (Circles != null)
			{
				for (var index = 0; index < Circles.Count; index++)
				{
					var circle = Circles[index];
					Handles.color = Color.white;// Color.Lerp(Color.blue, Color.red, index / (float)Circles.Count);
					Handles.color = Color.Lerp(Handles.color, Color.gray, onionColor01);
					if (circle.Radius > 2)
					{
						Gizmos.color = Handles.color;
						Gizmos.DrawSphere(circle.Position, .05f);
					}
					Handles.DrawWireDisc(circle.Position, Camera.current.transform.forward, circle.Radius);
				}
			}
#endif
		}
	}
}
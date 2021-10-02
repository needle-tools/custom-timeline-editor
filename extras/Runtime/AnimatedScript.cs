using System;
using System.Collections.Generic;
using System.Linq;
using Needle.Timeline;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Sample
{
#if UNITY_EDITOR
	public class TextTool : CustomClipToolBase
	{
		protected override bool OnSupports(Type type)
		{
			return typeof(string) == type;
		}

		protected override void OnAttach(VisualElement element)
		{
			base.OnAttach(element);
			var tf = new TextField() { style = { minWidth = 100 } };
			element.Add(tf);
			element.Add(new Button(() =>
			{
				var t = Targets.LastOrDefault();
				if (t.Clip is ICustomClip<string> str)
				{
					str.Add(new CustomKeyframe<string>(tf.text, (float)t.Time));
				}

			}){text = "Ok"});
		}

		protected override void OnInput(EditorWindow window)
		{
			
		}
	}
#endif

	public class AnimatedScript : MonoBehaviour, IAnimated
	{
		[Animate] public float MyValue;

		[Animate] private float MyOthervalue;

		[Animate] public int MyInt;

		[Animate] public double MyDouble;

		[Animate] public string MyString;

		// [Animate]
		// public List<Vector3> points = new List<Vector3>();
		//
		// private void OnDrawGizmosSelected()
		// {
		// 	var size = Vector3.up * .01f;
		// 	Gizmos.color = Color.yellow;
		// 	// Gizmos.DrawWireSphere();
		// 	for (var index = 1; index < points.Count; index++)
		// 	{
		// 		var pt = points[index];
		// 		Gizmos.DrawLine(pt, pt + size);
		// 	}
		// }
	}
}
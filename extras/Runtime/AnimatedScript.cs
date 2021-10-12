using System;
using System.Collections.Generic;
using System.Linq;
using Needle.Timeline;
using Needle.Timeline.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

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
			}) { text = "Ok" });
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

		[Animate] public List<MyType> CustomType;

		[Animate] public MyType SomeType;

		[Serializable]
		public class MyType
		{
			[PositionValue]
			public Vector3 Pos;
			public Color Color;
			public float Size;

			public static MyType GetRandom() => 
				new MyType { Pos = Random.insideUnitCircle, Color = Random.ColorHSV(), Size = Random.value * .5f + 0.02f };
		}
		
		
		
		
		private void OnDrawGizmos()
		{
			if (CustomType != null)
			{
				foreach (var e in CustomType)
				{
					Gizmos.color = e.Color;
					Gizmos.DrawSphere(e.Pos, e.Size);
				}
			}
			if (SomeType != null)
			{
				Gizmos.color = SomeType.Color;
				Gizmos.DrawSphere(SomeType.Pos + Vector3.right * 3, SomeType.Size);
			}
		}

#if UNITY_EDITOR
		public class MyTypeEditor : CustomClipToolBase
		{
			protected override bool OnSupports(Type type)
			{
				return typeof(MyType).IsAssignableFrom(type) || typeof(IList<MyType>).IsAssignableFrom(type);
			}

			protected override void OnAttach(VisualElement element)
			{
				base.OnAttach(element);
				element.Add(new Button(() =>
				{
					foreach (var t in Targets)
					{
						if (t.Clip is ICustomClip<List<MyType>> c)
						{
							var list = new List<MyType>();
							for (var i = 0; i < Random.Range(1, 5); i++)
								list.Add(MyType.GetRandom());
							c.Add(new CustomKeyframe<List<MyType>>(list, t.TimeF));
						}
						else if (t.Clip is ICustomClip<MyType> s)
						{
							s.Add(new CustomKeyframe<MyType>(MyType.GetRandom(), t.TimeF));
						}
					}
				}) { name = "Add" });
			}

			protected override void OnInput(EditorWindow window)
			{
			}
		}

#endif

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
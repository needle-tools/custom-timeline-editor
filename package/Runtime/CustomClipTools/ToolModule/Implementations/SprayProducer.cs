using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Needle.Timeline.CustomClipTools.ToolModule.Implementations
{
	[Priority(100)]
	public class SprayProducer : CoreToolModule
	{
		public SprayProducer() => AllowBinding = true;

		[PowerSlider(.1f, 20, 5)]
		public float Radius = 1;
		[Range(0,1000)]
		public int Max = 1000;
		[Range(0, 1)] public float Offset = 1;
		public bool OnSurface = false;
		public bool AllKeyframes = false;
		[Range(0,1), Tooltip("If OnSurface=False: flattens sprayed points in camera-view z")]
		public float DepthFactor = 1;

		protected override IList<Type> SupportedTypes { get; } = new[]
		{
			typeof(Vector3), typeof(Vector2), typeof(GameObject)
		};

		protected override IEnumerable<ICustomKeyframe?> GetKeyframes(ToolData toolData)
		{
			foreach (var kf in base.GetKeyframes(toolData))
			{
				if (IsCloseKeyframe(toolData, kf))
					yield return kf;
				else
					yield return CreateAndAddNewKeyframe(toolData, null);
			}

			if (AllKeyframes)
			{
				foreach (var kf in toolData.Clip.Keyframes)
				{
					var keyframe = kf as ICustomKeyframe;
					if (IsCloseKeyframe(toolData, keyframe)) continue;
					yield return keyframe;
				}
			}
		}

		protected override IEnumerable<ProducedValue> OnProduceValues(InputData input, ProduceContext context)
		{
			if (context.Count >= Max) yield break;
			if (input.WorldPosition == null) yield break;

			var isGameObject = typeof(GameObject).IsAssignableFrom(context.Type);
			if (isGameObject)
			{
				var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
				go.hideFlags = HideFlags.DontSaveInEditor;
				var pos = GetPosition(input, out var success);
				go.transform.position = pos;
				// if (context.Target is Component c) go.transform.SetParent(c.transform);
				yield return new ProducedValue(go, success);
			}
			else
			{
				yield return new ProducedValue(GetPosition(input, out var success), success);
			}
		}

		private Vector3 GetPosition(InputData input, out bool success)
		{

			if (OnSurface && input.WorldNormal != null)
			{
				var screenPoint = input.ScreenPosition + Random.insideUnitCircle * input.GetRadiusInPixel(Radius).Value;
				var ray = input.ToRay(screenPoint);
				if (Physics.Raycast(ray.origin, ray.direction, out var hit, Radius * 100))
				{
					Debug.DrawLine(hit.point, hit.point + hit.normal, Color.green, 1);
					success = true;
					return hit.point;
				}
			}
			else if(input.WorldPosition != null)
			{
				var offset = Random.insideUnitSphere;
				if (input.ViewRotation != null && Math.Abs(DepthFactor - 1) > 0.01f)
				{
					offset.z *= DepthFactor;
					offset = input.ViewRotation.Value * offset;
				}
				offset *= Radius;
				var pos = input.WorldPosition.Value + offset;
				pos += Offset * input.WorldNormal.GetValueOrDefault() * Radius;
				if (input.IsIn2DMode) pos.z = 0;
				Debug.DrawLine(pos, pos + Vector3.up * .05f, Color.white, 1);
				success = true;
				return pos;
			}
			success = false;
			return default;
		}
	}
}
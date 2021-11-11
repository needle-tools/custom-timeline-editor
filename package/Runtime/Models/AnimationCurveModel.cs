using System;
using System.Linq;
using UnityEngine;
// ReSharper disable MemberCanBePrivate.Global

namespace Needle.Timeline.Models
{
	[Serializable]
	public class AnimationCurveModel
	{
		public KeyModel[] KeyFrames;
		public WrapMode PostWrapMode;
		public WrapMode PreWrapMode;
		
		public class KeyModel
		{
			public float Time;
			public float Value;
			public WeightedMode WeightedMode;
			public float InWeight;
			public float OutWeight;
			public float InTangent;
			public float OutTangent;

			public KeyModel(Keyframe kf)
			{
				Time = kf.time;
				Value = kf.value;
				InTangent = kf.inTangent;
				OutTangent = kf.outTangent;
				InWeight = kf.inWeight;
				OutWeight = kf.outWeight;
				WeightedMode = kf.weightedMode;
			}
		}

		public AnimationCurve ToCurve()
		{
			var curve = new AnimationCurve(KeyFrames.Select(kf => new Keyframe(kf.Time, kf.Value, kf.InTangent, kf.OutTangent, kf.InWeight, kf.OutTangent)).ToArray());
			curve.postWrapMode = PostWrapMode;
			curve.preWrapMode = PreWrapMode;
			return curve;
		}
	}

	public static class AnimationCurveModelExtensions
	{
		public static AnimationCurveModel ToModel(this AnimationCurve curve)
		{
			if (curve == null) return null;
			var model = new AnimationCurveModel();
			model.PostWrapMode = curve.postWrapMode;
			model.PreWrapMode = curve.preWrapMode;
			model.KeyFrames = curve.keys.Select(kf => new AnimationCurveModel.KeyModel(kf)).ToArray();
			return model;
		}
	}
}
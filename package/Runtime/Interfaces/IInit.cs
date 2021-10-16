﻿#nullable enable
using UnityEngine;

namespace Needle.Timeline
{
	public enum InitStage
	{
		InstanceCreated = 0,
		BasicValuesSet = 1,
	}
	
	public interface IToolData
	{
		Vector3? WorldPosition { get; }
		Vector3? DeltaWorld { get; }
		Vector2 ToScreenPoint(Vector3 worldPoint);
	}

	public interface IInit
	{
		void Init(InitStage stage, IToolData? data);
	}
}
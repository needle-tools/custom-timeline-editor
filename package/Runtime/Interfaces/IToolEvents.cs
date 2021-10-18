#nullable enable
using UnityEngine;

namespace Needle.Timeline
{
	public enum ToolStage
	{
		InstanceCreated = 0,
		BasicValuesSet = 1,
		InputUpdated = 4,
		InputEnded = 5,
	}
	
	public interface IToolData
	{
		Vector3? StartWorldPosition { get; }
		Vector3? WorldPosition { get; }
		Vector3? DeltaWorld { get; }
		Vector2 ScreenPosition { get; }
		Vector2 ToScreenPoint(Vector3 worldPoint);
	}

	/// <summary>
	/// Implement on type to receive events when creating new instances
	/// </summary>
	public interface IToolEvents
	{
		void OnToolEvent(ToolStage stage, IToolData? data);
	}
}
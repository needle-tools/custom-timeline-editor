#nullable enable
using System;
using UnityEngine;

namespace Needle.Timeline
{
	public struct ToolData
	{
		public ICustomClip Clip;
		public object? Owner;
		public object? Value;
		public float Time;

		private bool _didSearchPosition;
		private Vector3? _position;
		public Vector3? Position
		{
			get
			{
				if (_didSearchPosition) return _position;
				_didSearchPosition = true;
				return _position = ToolHelpers.TryGetPosition(Owner, Value);
			}
		}
	}
}
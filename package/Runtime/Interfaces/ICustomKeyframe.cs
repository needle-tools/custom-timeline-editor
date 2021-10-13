using System;
using System.Collections.Generic;
using UnityEngine;

namespace Needle.Timeline
{
	public interface IReadonlyCustomKeyframe
	{
		object value { get; }
		float time { get; }
	}
	
	public interface ICustomKeyframe : IReadonlyCustomKeyframe
	{
		new object value { get; set; }
		new float time { get; set; }
		event Action TimeChanged, ValueChanged;
		void RaiseValueChangedEvent();
		Type[] AcceptedTypes();
	}
	
	public interface ICustomKeyframe<T> : ICustomKeyframe
	{
		new T value { get; set; }
	}
}
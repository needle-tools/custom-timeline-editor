using System;
using System.Collections.Generic;
using DefaultNamespace;
using JetBrains.Annotations;
using UnityEditor.Build;
using UnityEngine;

namespace Needle.Timeline
{
	public interface ICustomClip
	{
		object Evaluate(float time);
	}

	public interface ICustomClip<out T> : ICustomClip
	{
		new T Evaluate(float time);
	}
}
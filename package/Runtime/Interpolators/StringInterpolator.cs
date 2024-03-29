﻿using System;
using System.Text;

namespace Needle.Timeline
{
	public class StringInterpolator : IInterpolator<string>
	{
		private readonly StringBuilder sb = new StringBuilder();
		
		public string Interpolate(string v0, string v1, float t)
		{
			t = 1 - t;
			sb.Clear();
			for (var i = 0; i < v0?.Length; i++)
			{
				var _t = i / (float)v0.Length;
				if (_t >= t) break;
				sb.Append(v0[i]);
			}
			for (var i = 0; i < v1?.Length; i++)
			{
				var _t = i / (float)v1.Length;
				if (_t < t) continue;
				sb.Append(v1[i]);
			}
			
			return sb.ToString();
		}

		public object Instance { get; set; }

		public bool CanInterpolate(Type type)
		{
			return type == typeof(string);
		}

		public object Interpolate(object v0, object v1, float t)
		{
			return Interpolate((string)v0, (string)v1, t);
		}
	}
}
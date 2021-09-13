using System.Text;

namespace Needle.Timeline.Interpolators
{
	public class StringInterpolator : ICanInterpolate<string>
	{
		private readonly StringBuilder sb = new StringBuilder();
		
		public string Interpolate(string v0, string v1, float t)
		{
			t = 1 - t;
			sb.Clear();
			for (var i = 0; i < v0.Length; i++)
			{
				var _t = i / (float)v0.Length;
				if (_t >= t) break;
				sb.Append(v0[i]);
			}
			for (var i = 0; i < v1.Length; i++)
			{
				var _t = i / (float)v1.Length;
				if (_t < t) continue;
				sb.Append(v1[i]);
			}
			
			return sb.ToString();
		}
	}
}
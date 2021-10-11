using System;

namespace Needle.Timeline
{
	public class Priority : Attribute
	{
		/// <summary>
		/// Higher rating is chosen earlier
		/// </summary>
		public readonly int Rating;

		public Priority(int rating)
		{
			Rating = rating;
		}
	}
}
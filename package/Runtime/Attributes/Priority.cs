using System;

namespace Needle.Timeline
{
	public class Priority : Attribute, IPriority
	{
		/// <summary>
		/// Higher rating is chosen earlier
		/// </summary>
		public readonly int Rating;

		public Priority(int rating)
		{
			Rating = rating;
		}

		int IPriority.Priority
		{
			get => Rating; 
			set => throw new NotSupportedException();
		}
	}
}
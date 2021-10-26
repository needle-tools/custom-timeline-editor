using System;
using System.Collections;
using System.Collections.Generic;

namespace Needle.Timeline
{
	// how could we represent something that contains of multiple properties in a compute buffer
	// e.g. we have two buffers, one with the count per element and one with the sequential elements
	// buffer with counts: [1, 5, 3]
	// content: [1, 5, 5, 5, 5, 3, 3, 3] same number just shows that it is the same type
	
	
	internal interface IFlatten
	{
		bool CanRepresent(int stride);
		int Count { get; }
		void Insert(IList list, int expectedStride);
	}

	internal class MyType : IFlatten
	{
		public List<float> DynamicList;
		
		
		public bool CanRepresent(int stride)
		{
			throw new NotImplementedException();
		}

		public int Count { get; }
		
		public void Insert(IList list, int expectedStride)
		{
			throw new NotImplementedException();
		}
	}
	
	
	// maybe not necessary, we could also just require an group id per type
	// and then fill the two buffers the same way (maybe the first buffer contains int2 -> x = offset, y = count)

	internal interface IFlatten2
	{
		int GroupId { get; }
	}
	
	// and then we have 
	// [(0, 3), (3,2), (5,1)]
	// [3,3,3, 2,2, 1]
	// we can now dispatch threads with x = group index, y = index in group

}
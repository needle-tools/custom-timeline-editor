using UnityEngine;

namespace Needle.Timeline.ResourceProviders
{
	public interface IComputeBufferDescription : IUnityObjectDescription
	{
		int Size { get; set; }
		int Stride { get; set; }
		ComputeBufferType Type { get; set; }
		ComputeBufferMode Mode { get; set; }
		uint? CounterValue { get; set; }
	}

	public struct ComputeBufferDescription : IComputeBufferDescription
	{
		public string Name { get; set; }
		public HideFlags HideFlags { get; set; }
		public int Size { get; set; }
		public int Stride { get; set; }
		public ComputeBufferType Type { get; set; }
		public ComputeBufferMode Mode { get; set; }
		public uint? CounterValue { get; set; }
		
		public static ComputeBufferDescription Default(int size, int stride)
		{
			return new ComputeBufferDescription() { Size = size, Stride = stride };
		}
	}
}
// using System.Collections.Generic;
// using Needle.Timeline.Interfaces;
// using Unity.Collections;
// using UnityEditor;
// using UnityEngine;
//
// namespace Needle.Timeline
// {
// 	public class NativeSliceToListConverter<T> : ICanConvert<NativeArray<T>, IList<T>> where T : struct
// 	{
// 		private List<Vector3> buffer;
// 		
// 		public IList<T> Convert(NativeArray<T> input)
// 		{
// 			input.Reinterpret<Vector3>().CopyTo()
// 		}
// 	}
// }
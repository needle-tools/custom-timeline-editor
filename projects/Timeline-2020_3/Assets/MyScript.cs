using System;
using UnityEngine;

namespace DefaultNamespace
{
	[ExecuteAlways]
	public class MyScript : MonoBehaviour
	{
		private void OnEnable()
		{
			Debug.Log("HELLO");
		}
	}
}
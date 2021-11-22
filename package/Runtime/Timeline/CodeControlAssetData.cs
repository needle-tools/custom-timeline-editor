using System.Collections.Generic;
using UnityEngine;

namespace Needle.Timeline
{
	[CreateAssetMenu(menuName = "Needle/Timeline-Clip")]
	public class CodeControlAssetData : ScriptableObject
	{
		public List<JsonContainer> ClipData = new List<JsonContainer>();
	}
}
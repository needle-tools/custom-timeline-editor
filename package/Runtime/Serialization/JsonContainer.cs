using UnityEngine;

namespace Needle.Timeline
{
	public class JsonContainer : ScriptableObject
	{
		[field:SerializeField]
		public string Id { get; internal set; }
		[field:SerializeField]
		public string Content { get; internal set; }
	}
}
using UnityEngine;

namespace Needle.Timeline.ResourceProviders
{
	public interface IUnityObjectDescription
	{
		string Name { get; set; }
		HideFlags HideFlags { get; set; }
	}
}
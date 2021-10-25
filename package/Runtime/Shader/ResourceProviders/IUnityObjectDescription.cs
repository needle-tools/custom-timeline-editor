using UnityEngine;

namespace Needle.Timeline.ResourceProviders
{
	public interface IUnityObjectDescription
	{
		string Name { get; }
		HideFlags HideFlags { get; }
	}
}
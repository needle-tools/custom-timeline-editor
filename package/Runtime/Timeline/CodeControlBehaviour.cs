using System;
using UnityEngine.Playables;

namespace Needle.Timeline
{
	[Serializable]
	public class CodeControlBehaviour : PlayableBehaviour
	{
		/// <summary>
		/// set from asset, provides access to view models
		/// </summary>
		internal CodeControlAsset asset;

		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
		}
	}
}
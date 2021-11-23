using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

namespace Needle.Timeline
{
	[Serializable]
	public class CodeControlBehaviour : PlayableBehaviour
	{
		// internal List<ClipInfoViewModel> viewModels;
		
		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
		}
	}
}
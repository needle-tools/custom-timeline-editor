using UnityEngine;
using UnityEngine.Playables;

namespace Needle.Timeline
{
	public class CodeControlTrackMixer : PlayableBehaviour
	{
		// NOTE: This function is called at runtime and edit time.  Keep that in mind when setting the values of properties.
		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
			// Debug.Log(playerData);
			var trackBinding = playerData as Light;
			// float finalIntensity = 0f;
			// Color finalColor = Color.black;

			if (!trackBinding)
				return;

			var inputCount = playable.GetInputCount ();

			for (var i = 0; i < inputCount; i++)
			{
				// float inputWeight = playable.GetInputWeight(i);
				// ScriptPlayable<LightControlBehaviour> inputPlayable = (ScriptPlayable<LightControlBehaviour>)playable.GetInput(i);
				// LightControlBehaviour input = inputPlayable.GetBehaviour();

				// Use the above variables to process each frame of this playable.
				// finalIntensity += input.intensity * inputWeight;
				// finalColor += input.color * inputWeight;
			}

			//assign the result to the bound object
			// trackBinding.intensity = finalIntensity;
			// trackBinding.color = finalColor;
		}
	}
}
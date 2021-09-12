using UnityEngine;
using UnityEngine.Playables;

public class LightControlAsset : PlayableAsset
{
	public LightControlBehaviour template;

	public override Playable CreatePlayable (PlayableGraph graph, GameObject owner) {
		var playable = ScriptPlayable<LightControlBehaviour>.Create(graph, template);
		return playable;
	}
}
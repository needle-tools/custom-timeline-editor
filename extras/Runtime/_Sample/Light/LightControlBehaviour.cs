using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class LightControlBehaviour : PlayableBehaviour
{
	public Color color = Color.white;
	public float intensity = 1f;
}
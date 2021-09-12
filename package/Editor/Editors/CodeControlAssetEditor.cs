using Needle.Timeline;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CodeControlAsset))]
public class CodeControlAssetEditor : UnityEditor.Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		var asset = target as CodeControlAsset;
		if (!asset) return;

		if (asset.model != null && asset.model.clip != null)
		{
			if (GUILayout.Button("Add keyframe"))
			{
				var clip = asset.model.clip;
				var clipBindings = AnimationUtility.GetCurveBindings(clip);
				for (var index = 0; index < clipBindings.Length; index++)
				{
					var binding = clipBindings[index];
					var curve = AnimationUtility.GetEditorCurve(clip, binding);
					curve.AddKey(Random.value, Random.value);
					AnimationUtility.SetEditorCurve(clip, binding, curve);
				}
			}
		}
		
		// var behaviour = asset.instance;
		// if (behaviour == null) return;
		// var type = behaviour.boundType;
		// if (type == null) EditorGUILayout.LabelField("Not bound");
		// else
		// {
		// 	EditorGUILayout.LabelField(type.FullName);
		// }
	}
}
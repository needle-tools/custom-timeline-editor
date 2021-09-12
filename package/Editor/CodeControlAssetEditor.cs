// using Needle.Timeline;
// using UnityEditor;
//
// [CustomEditor(typeof(CodeControlAsset))]
// public class CodeControlAssetEditor : Editor
// {
// 	public override void OnInspectorGUI()
// 	{
// 		base.OnInspectorGUI();
// 		var asset = target as CodeControlAsset;
// 		if (!asset) return;
// 		var behaviour = asset.instance;
// 		if (behaviour == null) return;
// 		var type = behaviour.boundType;
// 		if (type == null) EditorGUILayout.LabelField("Not bound");
// 		else
// 		{
// 			EditorGUILayout.LabelField(type.FullName);
// 		}
// 	}
// }
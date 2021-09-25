using UnityEditor;
using UnityEngine;

namespace Needle.Timeline
{
	[CustomPropertyDrawer(typeof(CodeControlAsset))]
	public class CodeControlBehaviourDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
		{
			int fieldCount = 1;
			return fieldCount * EditorGUIUtility.singleLineHeight;
		}
 
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
		{
			Debug.Log("Hello");
		}
	}
}
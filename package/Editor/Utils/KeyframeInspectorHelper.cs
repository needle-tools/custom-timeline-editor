using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Needle.Timeline
{
	public class KeyframeInspectorHelper : ScriptableObject
	{
		internal static KeyframeInspectorHelper _instance;
		private static UnityEditor.Editor _editor;

		public static bool Select(string name, ICustomKeyframe keyframe)
		{
			if (keyframe == null) return false;
			if (!_instance)
			{
				_instance = CreateInstance<KeyframeInspectorHelper>();
				_instance.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
			}
			
			_instance.name = "Keyframe";
			_instance.fieldName = name;
			_instance.keyframe = keyframe;
			var wasSelected = Selection.activeObject = _instance;
			Selection.activeObject = _instance;
			UnityEditor.Editor.CreateCachedEditor(_instance, typeof(KeyframeInspectorHelperEditor), ref _editor);
			_editor.Repaint();
			if(wasSelected && _editor is KeyframeInspectorHelperEditor ed) ed.InternalOnEnable();
			return true;
		}

		public static void Deselect(ICustomKeyframe keyframe = null)
		{
			if (!_instance) return;
			if (keyframe == null || _instance.keyframe == keyframe)
			{
				_instance.keyframe = null;
				if (Selection.activeObject == _instance)
					Selection.activeObject = null;
			}
		}


		internal string fieldName;
		internal ICustomClip clip;
		internal ICustomKeyframe keyframe;
	}
		
	public static class KeyframeExtensions
	{
		public static bool Select(this ICustomKeyframe kf, ICustomClip owner)
		{
			if (kf.IsSelected()) return true;
			return KeyframeInspectorHelper.Select(owner.Name, kf);
		}
		
		public static bool IsSelected(this ICustomKeyframe kf) => 
			KeyframeInspectorHelper._instance && KeyframeInspectorHelper._instance.keyframe == kf && 
			Selection.activeObject == KeyframeInspectorHelper._instance;
		
		public static bool AnySelected(this ICustomKeyframe _) => KeyframeInspectorHelper._instance && KeyframeInspectorHelper._instance.keyframe != null && Selection.activeObject == KeyframeInspectorHelper._instance;
		
		public static bool AnySelected() => KeyframeInspectorHelper._instance && KeyframeInspectorHelper._instance.keyframe != null && Selection.activeObject == KeyframeInspectorHelper._instance;
	}

}
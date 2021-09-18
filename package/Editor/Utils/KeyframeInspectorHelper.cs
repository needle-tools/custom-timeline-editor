using UnityEditor;
using UnityEngine;

namespace Needle.Timeline
{
	public class KeyframeInspectorHelper : ScriptableObject
	{
		internal static KeyframeInspectorHelper _instance;
		private static UnityEditor.Editor _editor;

		public static bool Select(ICustomKeyframe keyframe)
		{
			if (keyframe == null) return false;
			if (!_instance)
			{
				_instance = CreateInstance<KeyframeInspectorHelper>();
				_instance.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
			}

			if (_instance.keyframe == keyframe) return true;
			_instance.keyframe = keyframe;
			var wasSelected = Selection.activeObject = _instance;
			Selection.activeObject = _instance;
			UnityEditor.Editor.CreateCachedEditor(_instance, typeof(KeyframeInspectorHelperEditor), ref _editor);
			_editor.Repaint();
			if(wasSelected && _editor is KeyframeInspectorHelperEditor ed) ed.InternalOnEnable();
			return true;
		}

		internal ICustomKeyframe keyframe;
	}
		
	public static class KeyframeExtensions
	{
		public static bool IsSelected(this ICustomKeyframe kf) => 
			KeyframeInspectorHelper._instance && KeyframeInspectorHelper._instance.keyframe == kf && 
			Selection.activeObject == KeyframeInspectorHelper._instance;
		
		public static bool AnySelected(this ICustomKeyframe _) => KeyframeInspectorHelper._instance && KeyframeInspectorHelper._instance.keyframe != null && Selection.activeObject == KeyframeInspectorHelper._instance;
		
		public static bool AnySelected() => KeyframeInspectorHelper._instance && KeyframeInspectorHelper._instance.keyframe != null && Selection.activeObject == KeyframeInspectorHelper._instance;
	}

}
using System.Collections.Generic;
using System.Linq;
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
				_instance.name = "Keyframe";
			}
			
			if (!IsSelected(keyframe))
			{
				selectedKeyframes.Add((keyframe, null, name));
			}
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
			
			if (selectedKeyframes.Any(s => s.keyframe == keyframe))
			{
				selectedKeyframes.RemoveAll(e => e.keyframe == keyframe);
			}
			else if(keyframe == null) 
				selectedKeyframes.Clear();
			
			if (Selection.activeObject == _instance && selectedKeyframes.Count <= 0)
				Selection.activeObject = null;
		}

		public static bool IsSelected(ICustomKeyframe keyframe)
		{
			return selectedKeyframes.Any(s => s.keyframe == keyframe);
		}

		public static bool HasAnySelected() => selectedKeyframes.Count > 0;
		public static int SelectionCount => selectedKeyframes.Count;

		public static IEnumerable<ICustomKeyframe> EnumerateSelected()
		{
			foreach (var sel in selectedKeyframes) yield return sel.keyframe;
		}

		internal static readonly List<(ICustomKeyframe keyframe, ICustomClip clip, string field)> selectedKeyframes 
			= new List<(ICustomKeyframe keyframe, ICustomClip clip, string field)>();
	}
		
	public static class KeyframeExtensions
	{
		public static bool Select(this ICustomKeyframe kf, ICustomClip owner)
		{
			if (kf.IsSelected()) return true;
			return KeyframeInspectorHelper.Select(owner.Name, kf);
		}
		
		public static bool IsSelected(this ICustomKeyframe kf) => 
			KeyframeInspectorHelper._instance && KeyframeInspectorHelper.IsSelected(kf) && 
			Selection.activeObject == KeyframeInspectorHelper._instance;
		
		public static bool AnySelected(this ICustomKeyframe _) => 
			KeyframeInspectorHelper._instance && KeyframeInspectorHelper.selectedKeyframes != null && Selection.activeObject == KeyframeInspectorHelper._instance;
		
		public static bool AnySelected() => 
			KeyframeInspectorHelper._instance && KeyframeInspectorHelper.selectedKeyframes != null && Selection.activeObject == KeyframeInspectorHelper._instance;
	}

}
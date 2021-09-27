using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Needle.Timeline
{
	public class KeyframeSelector : ScriptableObject
	{
		internal static KeyframeSelector _instance;
		private static UnityEditor.Editor _editor;

		public static bool Select(ICustomClip clip, ICustomKeyframe keyframe)
		{
			if (keyframe == null) return false;
			if (!_instance)
			{
				_instance = CreateInstance<KeyframeSelector>();
				_instance.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
				_instance.name = "Keyframe";
			}
			
			if (!IsSelected(keyframe))
			{
				selectedKeyframes.Add(new SelectedKeyframe()
				{
					Clip = clip,
					Keyframe = keyframe,
				});
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
			
			if (selectedKeyframes.Any(s => s.Keyframe == keyframe))
			{
				selectedKeyframes.RemoveAll(e => e.Keyframe == keyframe);
			}
			else if(keyframe == null) 
				selectedKeyframes.Clear();
			
			if (Selection.activeObject == _instance && selectedKeyframes.Count <= 0)
				Selection.activeObject = null;
		}

		public static bool IsSelected(ICustomKeyframe keyframe)
		{
			return selectedKeyframes.Any(s => s.Keyframe == keyframe);
		}

		public static bool HasAnySelected() => selectedKeyframes.Count > 0;
		public static int SelectionCount => selectedKeyframes.Count;

		public static IEnumerable<ICustomKeyframe> EnumerateSelected()
		{
			foreach (var sel in selectedKeyframes) yield return sel.Keyframe;
		}

		internal static readonly List<SelectedKeyframe> selectedKeyframes = new List<SelectedKeyframe>();
	}
		
	public static class KeyframeExtensions
	{
		public static bool Select(this ICustomKeyframe kf, ICustomClip clip)
		{
			if (kf.IsSelected()) return true;
			return KeyframeSelector.Select(clip, kf);
		}
		
		public static bool IsSelected(this ICustomKeyframe kf) => 
			KeyframeSelector._instance && KeyframeSelector.IsSelected(kf) && 
			Selection.activeObject == KeyframeSelector._instance;
		
		public static bool AnySelected(this ICustomKeyframe _) => 
			KeyframeSelector._instance && KeyframeSelector.selectedKeyframes != null && Selection.activeObject == KeyframeSelector._instance;
		
		public static bool AnySelected() => 
			KeyframeSelector._instance && KeyframeSelector.selectedKeyframes != null && Selection.activeObject == KeyframeSelector._instance;
	}

}
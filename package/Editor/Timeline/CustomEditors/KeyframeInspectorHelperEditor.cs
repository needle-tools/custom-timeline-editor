using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Needle.Timeline
{
	[CustomEditor(typeof(KeyframeInspectorHelper))]
	public class KeyframeInspectorHelperEditor : UnityEditor.Editor
	{
		private void OnEnable()
		{
			InternalOnEnable();
		}

		internal void InternalOnEnable()
		{
			var helper = target as KeyframeInspectorHelper;
			if (helper == null) return;
			if (helper.keyframe != null)
				TryFindEditorWith(helper.keyframe);
		}

		public override void OnInspectorGUI()
		{
			var helper = target as KeyframeInspectorHelper;
			if (helper == null) return;
			if (helper.keyframe != null)
			{
				if (_customEditor != null)
				{
					_customEditor.Target = helper.keyframe;
					_customEditor.OnInspectorGUI();
				}
				else
				{
					CustomTimelineEditor.DrawDefaultInspector(helper.keyframe);
				}
			}
		}

		private CustomTimelineEditor _customEditor;

		private static readonly Dictionary<Type, CustomTimelineEditor> _customEditorsCache
			= new Dictionary<Type, CustomTimelineEditor>();

		private bool TryFindEditorWith(ICustomKeyframe currentKeyframe)
		{
			var keyframeType = currentKeyframe.GetType();
			if (!_customEditorsCache.TryGetValue(keyframeType, out var existing))
			{
				var editorType = typeof(CustomTimelineEditor);
				var types = TypeCache.GetTypesWithAttribute<CustomKeyframeEditorAttribute>();
				foreach (var type in types)
				{
					if (!editorType.IsAssignableFrom(type)) continue;
					foreach (var att in type.GetCustomAttributes<CustomKeyframeEditorAttribute>())
					{
						if (att.Type.IsAssignableFrom(keyframeType))
						{
							var editor = Activator.CreateInstance(type) as CustomTimelineEditor;
							_customEditorsCache.Add(keyframeType, editor);
							_customEditor = editor;
							return _customEditor != null;
						}
					}
				}

				return false;
			}

			_customEditor = existing;
			return _customEditor != null;
		}
	}
}
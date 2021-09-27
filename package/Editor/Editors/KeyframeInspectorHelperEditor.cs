using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Needle.Timeline
{
	[CustomEditor(typeof(KeyframeSelector))]
	public class KeyframeInspectorHelperEditor : UnityEditor.Editor
	{
		private void OnEnable()
		{
			InternalOnEnable();
		}

		internal void InternalOnEnable()
		{
			_currentEditors.Clear();
			foreach (var ed in KeyframeSelector.selectedKeyframes)
			{
				TryFindEditorWith(ed.Keyframe);
			}
		}

		public override void OnInspectorGUI()
		{
			if(KeyframeSelector.selectedKeyframes.Count != _currentEditors.Count)
				InternalOnEnable();
			
			
			if (KeyframeSelector.selectedKeyframes?.Count > 0)
			{
				for (var i = 0; i < KeyframeSelector.selectedKeyframes.Count; i++)
				{
					var sel = KeyframeSelector.selectedKeyframes[i];
					var ed = _currentEditors[i];
					if (ed == null)
					{
						CustomKeyframeEditorBase.DrawDefaultInspector(sel);
					}
					else
					{
						ed.Target.Clear();
						ed.Target.Add(sel);
						ed.OnInspectorGUI();
					}
				}
			}
		}

		private readonly IList<CustomKeyframeEditorBase> _currentEditors = new List<CustomKeyframeEditorBase>();

		#region Editors
		private static readonly Dictionary<Type, CustomKeyframeEditorBase> _customEditorsCache
			= new Dictionary<Type, CustomKeyframeEditorBase>();

		private bool TryFindEditorWith(ICustomKeyframe keyframe)
		{
			var keyframeType = keyframe.GetType();
			if (!_customEditorsCache.TryGetValue(keyframeType, out var existing))
			{
				var editorType = typeof(CustomKeyframeEditorBase);
				var types = TypeCache.GetTypesWithAttribute<CustomKeyframeEditorAttribute>();
				foreach (var type in types)
				{
					if (!editorType.IsAssignableFrom(type)) continue;
					foreach (var att in type.GetCustomAttributes<CustomKeyframeEditorAttribute>())
					{
						if (att.Type.IsAssignableFrom(keyframeType))
						{
							var editor = Activator.CreateInstance(type) as CustomKeyframeEditorBase;
							_customEditorsCache.Add(keyframeType, editor);
							_currentEditors.Add(editor);
							return true;
						}
					}
				}

				_currentEditors.Add(null);
				return false;
			}
			
			if (existing != null)
			{
				_currentEditors.Add(existing);
				return true;
			}

			return false;
		}
		#endregion
	}
}
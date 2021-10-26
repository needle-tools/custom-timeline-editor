using System.Security.AccessControl;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.LowLevel;

namespace Needle.Timeline
{
	[InitializeOnLoad]
	internal static class FixAlwaysRefresh
	{
		static FixAlwaysRefresh()
		{
			var go = new GameObject();
			go.SetActive(false);
			go.name = nameof(FixAlwaysRefresh);
			go.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.DontSaveInEditor;
			go.AddComponent(typeof(ParticleSystem));
			// EditorApplication.update += () =>
			// {
			// 	Debug.Log("Update");
			// 	EditorUtility.SetDirty(SceneView.lastActiveSceneView);
			// 	EditorApplication.QueuePlayerLoopUpdate();
			// 	SceneView.RepaintAll();
			// 	UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
			// 	TimelineEditor.inspectedDirector.Evaluate(); 
			// };
		}
	}
}
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Needle.Timeline
{
	[InitializeOnLoad]
	internal static class TimelineTempSaveHandler
	{
		static TimelineTempSaveHandler()
		{
			AssemblyReloadEvents.beforeAssemblyReload += BeforeReload;
			EditorApplication.wantsToQuit += OnWantsToQuit;
		}

		private static bool OnWantsToQuit()
		{
			TempFileLocationLoader.DeleteTempUnsavedChangesDirectory();
			return CheckUnsavedChanges();
		}

		private static bool CheckUnsavedChanges()
		{ 
			if (hasUnsavedChanges || ClipInfoViewModel.Instances.Any(vm => vm.HasUnsavedChanges))
			{
				if (EditorUtility.DisplayDialog("Unsaved Timeline Changes", "You have unsaved timeline changes", "Save", "Exit without saving"))
				{
					var loader = LoadersRegistry.GetDefault();
					foreach (var vm in ClipInfoViewModel.Instances)
					{
						vm.Save(loader);
					}
				}
			}
			return true;
		}

		private static bool hasUnsavedChanges
		{
			get => SessionState.GetBool(nameof(TimelineTempSaveHandler) + ".DidHaveUnsavedChanges", false);
			set => SessionState.SetBool(nameof(TimelineTempSaveHandler) + ".DidHaveUnsavedChanges", value);
		}

		private static void BeforeReload()
		{
			TempFileLocationLoader loader = null;
			hasUnsavedChanges = false;
			
			foreach (var vm in ClipInfoViewModel.Instances)
			{
				if (vm.HasUnsavedChanges)
				{
					hasUnsavedChanges = true;
					if (loader == null)
					{
						Debug.Log("BEFORE RELOAD");
						loader = new TempFileLocationLoader(null);
					}
					Debug.Log("SHOULD SAVE: " + vm.Id); 
					foreach (var clip in vm.clips)
					{
						loader.Save(clip.Id, new SerializationContext(vm.TimelineClip, vm.asset), clip);
					}
				}
			}
		}
	}
}
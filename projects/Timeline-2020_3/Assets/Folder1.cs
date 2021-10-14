using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class Folder1 : MonoBehaviour
{
    [ContextMenu(nameof(ToggleModeNow))]
    private void ToggleModeNow()
    {
        ToggleMode();
    }

    private void OnEnable()
    {
        Debug.Log(EditorApplication.applicationContentsPath);
        Debug.Log(string.Join(", ", ModeService.modeNames));
        // ToggleMode();
    }

    [ContextMenu(nameof(SetMyMode))]
    private void SetMyMode() => SetMode("mymode123");

    private static void SetMode(string name)
    {
        ModeService.ChangeModeById(name);
    }

    [MenuItem("Mode/Next")]
    private static void ToggleMode()
    {
        var cur = ModeService.currentIndex;
        var next = (cur + 1) % (ModeService.modeCount-1);
        Debug.Log(next);
        var id = ModeService.modeNames[next];
        Debug.Log("Enable mode " + id + ", " + next); 
        ModeService.ChangeModeById(id);
    }
}

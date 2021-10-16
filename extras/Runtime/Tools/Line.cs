using System.Data.Common;
using Mono.Cecil;
using Needle.Timeline;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;


public struct Line : IModifySelf
{
	public Vector3 Start;
	public Vector3 End;
	
	public bool OnInput()
	{
		var start = Handles.PositionHandle(Start, Quaternion.identity);
		var end = Handles.PositionHandle(End, Quaternion.identity);
		var changed = start != Start || end != End;
		Start = start;
		End = end;
		return changed;
	}
}